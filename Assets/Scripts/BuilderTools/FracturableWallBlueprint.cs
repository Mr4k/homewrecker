using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using VoronoiLib;
using VoronoiLib.Structures;

class FracturableWallBlueprint : BaseBlueprint
{
    public Vector3 WallSize;
    public int NumShatterPoints = 1;
    public float minPointFreeRadiusPct = 0.05f;
    public LinkedList<VEdge> edges = new LinkedList<VEdge>();
    public Material MaterialForMesh;

    public override void RefreshBlueprint()
    {
        Debug.Log("refreshing!");
        CreateFractureMesh(WallSize.x, WallSize.y, NumShatterPoints, Math.Min(WallSize.x, WallSize.y) * minPointFreeRadiusPct);
    }

    public void CreateFractureMesh(float sizeX, float sizeY, int numShatterPoints, float minPointFreeRadius)
    {
        // generate vornoi points
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < numShatterPoints; i++)
        {
            Vector2 point = -Vector2.one;
            bool pointAccepted = false;
            int numItersBeforeFailure = 100;
            for (int j = 0; j < numItersBeforeFailure; j++)
            {
                point = new Vector2(UnityEngine.Random.Range(-sizeX / 2, sizeX / 2), UnityEngine.Random.Range(-sizeY / 2, sizeY / 2));
                // n^2 but who cares it's baked
                pointAccepted = true;
                foreach (var otherPoint in points)
                {
                    if (Vector2.Distance(point, otherPoint) <= minPointFreeRadius)
                    {
                        pointAccepted = false;
                        break;
                    }
                }
                if (pointAccepted)
                {
                    break;
                }
            }
            if (!pointAccepted)
            {
                throw new Exception("Could not place point " + (i + 1) + " / " + numShatterPoints + " after " + numItersBeforeFailure + " iterations");
            }
            points.Add(point);
        }
        Debug.Log("got points " + points.Count());

        double minX = -sizeX / 2;
        double minY = -sizeY / 2;
        double maxX = sizeX / 2;
        double maxY = sizeY / 2;
        edges = FortunesAlgorithm.Run(points.Select(p => new FortuneSite(p.x, p.y)).ToList(), minX, minY, maxX, maxY);
        EmbedVornoiGraphInRectangle(edges, minX, minY, maxX, maxY);
        Debug.Log("got edges " + edges);
        foreach (var e in edges)
        {
            Debug.Log("edge " + e.Start + "," + e.End);
        }
        var polygons = GetInteriorPolygonsFromConnectedEmbeddedPlanarGraph(edges);
        foreach (var p in polygons)
        {
            Debug.Log("Polygon:" + string.Join(",", p.Select(p => p.ToString())));
        }
        var triangulatedPolygons = new List<TriangluatedPolygon>();
        foreach (var p in polygons)
        {
            triangulatedPolygons.Add(CreateWallChunkTriangleMesh(p, transform.right, -transform.up, transform.forward, WallSize.z));
        }

        var outputContainer = GetComponentInChildren<BlueprintOutputContainer>();
        if (outputContainer != null)
        {
            DestroyImmediate(outputContainer.gameObject);
        }
        var containerGameObject = new GameObject("BlueprintOutputContainer", typeof(BlueprintOutputContainer));
        containerGameObject.transform.SetParent(transform);
        containerGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (var tp in triangulatedPolygons)
        {
            var mesh = MeshFromTriangulatedPolygon(tp);
            GameObject gameObject = new GameObject();
            gameObject.transform.SetPositionAndRotation(Vector3.zero, quaternion.identity);
            gameObject.transform.SetParent(containerGameObject.transform, false);

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = MaterialForMesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }

    public struct TriangluatedPolygon
    {
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<int> indexes;
    }
    public static TriangluatedPolygon TriangulateConvexPolygon(List<Vector2> polygon, Vector3 right, Vector3 down)
    {
        // use a spoke triangulation at a new centroid point b/c it seems better than fan triangulation for rendering / collision
        Vector2 center2d = Vector2.zero;
        foreach (var pt in polygon)
        {
            center2d += pt;
        }
        center2d /= polygon.Count;
        List<Vector3> verts3d = new List<Vector3>();
        foreach (var vert2d in polygon)
        {
            verts3d.Add(right * vert2d.x + down * vert2d.y);
        }
        verts3d.Add(center2d.x * right + center2d.y * down);
        int centerIdx = verts3d.Count - 1;
        List<int> triangleIndexes = new List<int>();
        for (int i = 0; i < polygon.Count; i++)
        {
            triangleIndexes.Add(i);
            triangleIndexes.Add((i + 1) % polygon.Count);
            triangleIndexes.Add(centerIdx);
        }
        return new TriangluatedPolygon
        {
            vertices = verts3d,
            indexes = triangleIndexes,
        };
    }

    public static TriangluatedPolygon CreateWallChunkTriangleMesh(List<Vector2> polygon, Vector3 right, Vector3 down, Vector3 forward, float depth)
    {
        // use a spoke triangulation at a new centroid point b/c it seems better than fan triangulation for rendering / collision
        Vector2 center2d = Vector2.zero;
        foreach (var pt in polygon)
        {
            center2d += pt;
        }
        center2d /= polygon.Count;

        List<Vector3> frontFaceVerts3d = new List<Vector3>();
        List<Vector3> frontFaceVertNormals = new List<Vector3>();
        List<Vector3> backFaceVerts3d = new List<Vector3>();
        List<Vector3> backFaceVertNormals = new List<Vector3>();
        var frontFaceNormal = new Vector3(0, 0, -1);
        var backFaceNormal = new Vector3(0, 0, 1);
        // construct the front and back faces
        foreach (var vert2d in polygon)
        {
            var frontVertexPos = right * vert2d.x + down * vert2d.y - forward * depth / 2;
            var backVertexPos = right * vert2d.x + down * vert2d.y + forward * depth / 2;
            frontFaceVerts3d.Add(frontVertexPos);
            frontFaceVertNormals.Add(frontFaceNormal);
            backFaceVerts3d.Add(backVertexPos);
            backFaceVertNormals.Add(backFaceNormal);
        }
        backFaceVerts3d.Reverse();
        frontFaceVerts3d.Add(center2d.x * right + center2d.y * down - forward * depth / 2);
        frontFaceVertNormals.Add(frontFaceNormal);
        backFaceVerts3d.Add(center2d.x * right + center2d.y * down + forward * depth / 2);
        backFaceVertNormals.Add(backFaceNormal);

        int centerIdx = frontFaceVerts3d.Count - 1;
        List<int> frontTriangleIndexes = new List<int>();
        List<int> backTriangleIndexes = new List<int>();
        for (int i = 0; i < polygon.Count; i++)
        {
            int nextI = (i + 1) % polygon.Count;
            frontTriangleIndexes.Add(i);
            frontTriangleIndexes.Add(nextI);
            frontTriangleIndexes.Add(centerIdx);

            int backTriangleVertexOffset = frontFaceVerts3d.Count;
            backTriangleIndexes.Add(i + backTriangleVertexOffset);
            backTriangleIndexes.Add(nextI + backTriangleVertexOffset);
            backTriangleIndexes.Add(centerIdx + backTriangleVertexOffset);
        }

        List<Vector3> sideFaceVerts3d = new List<Vector3>();
        List<Vector3> sideFaceVertNormals = new List<Vector3>();
        List<int> sideTriangleIndexes = new List<int>();
        int sideTriangleVertexOffset = frontFaceVerts3d.Count + backFaceVerts3d.Count;

        // construct the side faces
        for (int i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            var aFrontVertexPos = right * a.x + down * a.y - forward * depth / 2;
            var aBackVertexPos = right * a.x + down * a.y + forward * depth / 2;
            var bFrontVertexPos = right * b.x + down * b.y - forward * depth / 2;
            var bBackVertexPos = right * b.x + down * b.y + forward * depth / 2;
            var normal = Vector3.Cross(aFrontVertexPos - bFrontVertexPos, bBackVertexPos - bFrontVertexPos);

            sideFaceVerts3d.Add(aFrontVertexPos);
            sideFaceVerts3d.Add(aBackVertexPos);
            sideFaceVerts3d.Add(bFrontVertexPos);
            sideFaceVerts3d.Add(bBackVertexPos);
            sideFaceVertNormals.Add(normal);
            sideFaceVertNormals.Add(normal);
            sideFaceVertNormals.Add(normal);
            sideFaceVertNormals.Add(normal);

            int startIdx = i * 4 + sideTriangleVertexOffset;
            sideTriangleIndexes.Add(startIdx + 3);
            sideTriangleIndexes.Add(startIdx + 2);
            sideTriangleIndexes.Add(startIdx);
        }

        return new TriangluatedPolygon
        {
            vertices = frontFaceVerts3d.Concat(backFaceVerts3d).Concat(sideFaceVerts3d).ToList(),
            normals = frontFaceVertNormals.Concat(backFaceVertNormals).Concat(sideFaceVertNormals).ToList(),
            indexes = frontTriangleIndexes.Concat(backTriangleIndexes).Concat(sideTriangleIndexes).ToList()
        };
    }

    public static void EmbedVornoiGraphInRectangle(LinkedList<VEdge> graph, double minX, double minY, double maxX, double maxY)
    {
        // TODO maybe figure out what to do with corners
        List<VPoint> leftEdgeResults = new List<VPoint>();
        List<VPoint> rightEdgeResults = new List<VPoint>();
        List<VPoint> topEdgeResults = new List<VPoint>();
        List<VPoint> bottomEdgeResults = new List<VPoint>();

        // assumption, we pray points only appear once in the output
        HashSet<VPoint> points = new HashSet<VPoint>();
        foreach (var edge in graph)
        {
            points.Add(edge.Start);
            points.Add(edge.End);
        }

        foreach (var pt in points)
        {
            // note it's possible for a point to be in multiple lists here and I don't think that's a bad thing
            // but if we somehow happen to have an exact corner things might get spicy
            if (pt.X.ApproxEqual(minX))
            {
                leftEdgeResults.Add(pt);
            }
            if (pt.Y.ApproxEqual(minY))
            {
                topEdgeResults.Add(pt);
            }
            if (pt.X.ApproxEqual(maxX))
            {
                rightEdgeResults.Add(pt);
            }
            if (pt.Y.ApproxEqual(maxY))
            {
                bottomEdgeResults.Add(pt);
            }
        }

        var topLeftCorner = new VPoint(minX, minY);
        var topRightCorner = new VPoint(maxX, minY);
        var bottomRightCorner = new VPoint(maxX, maxY);
        var bottomLeftCorner = new VPoint(minX, maxY);

        leftEdgeResults.Sort((a, b) => a.Y.CompareTo(b.Y));
        rightEdgeResults.Sort((a, b) => a.Y.CompareTo(b.Y));
        topEdgeResults.Sort((a, b) => a.X.CompareTo(b.X));
        bottomEdgeResults.Sort((a, b) => a.X.CompareTo(b.X));

        leftEdgeResults.Insert(0, topLeftCorner);
        leftEdgeResults.Add(bottomLeftCorner);

        rightEdgeResults.Insert(0, topRightCorner);
        rightEdgeResults.Add(bottomRightCorner);

        topEdgeResults.Insert(0, topLeftCorner);
        topEdgeResults.Add(topRightCorner);

        bottomEdgeResults.Insert(0, bottomLeftCorner);
        bottomEdgeResults.Add(bottomRightCorner);

        List<List<VPoint>> listsToProcess = new List<List<VPoint>>
        {
            leftEdgeResults,
            rightEdgeResults,
            topEdgeResults,
            bottomEdgeResults
        };

        foreach (var l in listsToProcess)
        {
            for (int i = 0; i < l.Count - 1; i++)
            {
                var curr = l[i];
                var next = l[i + 1];
                // TODO switch away from vedge to a type with less info we don't care about the dual
                var fakeEdge = new VEdge(curr, new FortuneSite(0, 0), new FortuneSite(0, 0))
                {
                    End = next
                };
                graph.AddLast(fakeEdge);
            }
        }
    }

    struct EmbeddedVertex
    {
        public VPoint EmbeddedPoint;
        public List<Tuple<double, EmbeddedVertex>> LinkedVerts;
    }

    public static List<List<Vector2>> GetInteriorPolygonsFromConnectedEmbeddedPlanarGraph(LinkedList<VEdge> graph)
    {
        // assumption, we pray points only appear once in the output
        Dictionary<VPoint, EmbeddedVertex> embeddedVerts = new Dictionary<VPoint, EmbeddedVertex>();

        // create graph with no connections
        foreach (var edge in graph)
        {
            if (!embeddedVerts.ContainsKey(edge.Start))
            {
                embeddedVerts[edge.Start] = new EmbeddedVertex
                {
                    EmbeddedPoint = edge.Start,
                    LinkedVerts = new List<Tuple<double, EmbeddedVertex>>(),
                };
            }
            if (!embeddedVerts.ContainsKey(edge.End))
            {
                embeddedVerts[edge.End] = new EmbeddedVertex
                {
                    EmbeddedPoint = edge.End,
                    LinkedVerts = new List<Tuple<double, EmbeddedVertex>>(),
                };
            }
        }

        // add connections
        foreach (var edge in graph)
        {
            var start = embeddedVerts[edge.Start];
            var end = embeddedVerts[edge.End];
            var angleStartToEnd = Math.Atan2(end.EmbeddedPoint.Y - start.EmbeddedPoint.Y, end.EmbeddedPoint.X - start.EmbeddedPoint.X);
            start.LinkedVerts.Add(Tuple.Create(angleStartToEnd, end));
            var angleEndToStart = Math.Atan2(start.EmbeddedPoint.Y - end.EmbeddedPoint.Y, start.EmbeddedPoint.X - end.EmbeddedPoint.X);
            end.LinkedVerts.Add(Tuple.Create(angleEndToStart, start));
        }

        // sort each verts connections by angle in clockwise order
        foreach (var vert in embeddedVerts.Values)
        {
            vert.LinkedVerts.Sort((a, b) =>
            {
                var aAng = a.Item1;
                var bAng = b.Item1;
                return aAng.CompareTo(bAng);
            });
        }

        List<List<Vector2>> polygons = new List<List<Vector2>>();
        // assumption each directed edge can only be used once
        // each undirected edge is used 2x but note this will mean we will get a outer face as well
        HashSet<Tuple<EmbeddedVertex, EmbeddedVertex>> seenDirectedEdges = new HashSet<Tuple<EmbeddedVertex, EmbeddedVertex>>();

        // n^2 I think but whatever
        foreach (var vert in embeddedVerts.Values)
        {
            Tuple<double, EmbeddedVertex> outGoingConnection = null;
            for (int i = 0; i < vert.LinkedVerts.Count; i++)
            {
                var outEdge = Tuple.Create(vert, vert.LinkedVerts[i].Item2);
                if (seenDirectedEdges.Contains(outEdge))
                {
                    continue;
                }
                outGoingConnection = vert.LinkedVerts[i];
                seenDirectedEdges.Add(outEdge);
                break;
            }
            if (outGoingConnection == null)
            {
                continue;
            }

            List<Vector2> polygon = new List<Vector2>()
            {
                new Vector2((float)vert.EmbeddedPoint.X, (float)vert.EmbeddedPoint.Y),
            };
            var prevVert = vert;
            var (_, currVert) = outGoingConnection;
            while (currVert.EmbeddedPoint != vert.EmbeddedPoint)
            {
                polygon.Add(new Vector2((float)currVert.EmbeddedPoint.X, (float)currVert.EmbeddedPoint.Y));
                // find this vertex in their list
                int index = -1;
                for (int i = 0; i < currVert.LinkedVerts.Count; i++)
                {
                    if (currVert.LinkedVerts[i].Item2.EmbeddedPoint == prevVert.EmbeddedPoint)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    throw new Exception("could not find matching index for vertex");
                }

                // this is the vertex "right after" the current edge
                int nextIndex = index - 1;
                if (nextIndex < 0)
                {
                    nextIndex += currVert.LinkedVerts.Count;
                }

                prevVert = currVert;
                currVert = currVert.LinkedVerts[nextIndex].Item2;
                var outEdge = Tuple.Create(prevVert, currVert);
                if (seenDirectedEdges.Contains(outEdge))
                {
                    throw new Exception("next edge in face has already been seen");
                }
                seenDirectedEdges.Add(outEdge);
            }
            // exclude the outside face
            if (DeterminePolygonWindingOrder(polygon) > 0)
            {
                polygons.Add(polygon);
            }
        }
        return polygons;
    }

    public static int DeterminePolygonWindingOrder(List<Vector2> polygon)
    {
        // a closed polygon must have at least 3 non colinear points
        var a = polygon[0];
        var b = polygon[1];
        var c = polygon[2];
        Vector3 e1 = b - a;
        Vector3 e2 = c - b;
        return Math.Sign(Vector3.Cross(e1, e2).z);
    }

    public static Mesh MeshFromTriangulatedPolygon(TriangluatedPolygon poly)
    {
        var mesh = new Mesh
        {
            vertices = poly.vertices.ToArray(),
            triangles = poly.indexes.ToArray(),
            normals = poly.normals.ToArray(),
        };
        return mesh;
    }

    private void OnDrawGizmos()
    {
        int i = 0;
        Color[] colors = new Color[] { Color.white, Color.rebeccaPurple, Color.yellowGreen, Color.red, Color.darkCyan, Color.darkGoldenRod, Color.darkGreen };
        foreach (var e in edges)
        {
            Gizmos.color = colors[i % colors.Length];
            Vector3 start = (float)e.Start.X * transform.right + (float)e.Start.Y * -transform.up + transform.position;
            Vector3 end = (float)e.End.X * transform.right + (float)e.End.Y * -transform.up + transform.position;
            Gizmos.DrawLine(start, end);
            i++;
        }
    }
}