using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Sliceable : MonoBehaviour
{
    public GameObject SliceablePrefab;
    public void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshCollider = GetComponent<MeshCollider>();
        if (meshFilter.sharedMesh != meshCollider.sharedMesh)
        {
            throw new Exception("meshFilter sharedMesh does not equal meshCollider sharedMesh");
        }
        if (!meshCollider.convex)
        {
            throw new Exception("mesh must be convex (the note collider being convex is just a proxy for this)");
        }
    }

    private void ParitionVerticies()
    {

    }

    public void Slice(Vector3 cameraPosition, Vector3 startPoint, Vector3 endPoint, float maxSliceRange)
    {
        cameraPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(cameraPosition);
        startPoint = transform.worldToLocalMatrix.MultiplyPoint3x4(startPoint);
        endPoint = transform.worldToLocalMatrix.MultiplyPoint3x4(endPoint);

        // construct the side bounds
        Vector3[] anchorBoundNormals = new Vector3[2];
        Vector3[] anchorPoints = new Vector3[]
        {
            startPoint,
            endPoint,
        };
        Vector3 originToStart = startPoint - cameraPosition;
        Vector3 cutPlaneNormal = Vector3.Cross(endPoint - startPoint, originToStart);
        cutPlaneNormal.Normalize(); // not strictly needed but might help us with stability

        // now we assign vertices to a side or out of bounds
        // if there are out of bounds verts on both sides refuse to cut I think
        // assuming the mesh is convex then that means the cut wasn't all the way through?

        var meshFilter = GetComponent<MeshFilter>();
        var meshCollider = GetComponent<MeshCollider>();
        var mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Dictionary<int, float> signedVertDistAlongCutNormal = new Dictionary<int, float>();
        for (var i = 0; i < vertices.Length; i++)
        {
            Vector3 vert = vertices[i];
            // figure out which side of the cut plane we are on
            Vector3 vertFromStart = vert - startPoint;
            float signedDistanceAlongCutNormal = Vector3.Dot(vertFromStart, cutPlaneNormal);
            signedVertDistAlongCutNormal.Add(i, signedDistanceAlongCutNormal);
        }

        // cut the mesh
        // broken verts will form the faces we need to construct
        // uhhh maybe caps seem a little complicated
        // feels like one way is to take a string and then wind it around the polygon on the plane

        const int TOP_PARTION_IDX = 0;
        const int BOTTOM_PARTION_IDX = 1;
        List<Vector3>[] partitionMeshVerts = new List<Vector3>[2];
        List<Vector3>[] partitionMeshNormals = new List<Vector3>[2];
        List<int>[] partitionCapIndexes = new List<int>[2];
        Dictionary<int, int>[] sameSideDirectVertexMapping = new Dictionary<int, int>[2];
        List<int>[] paritionMeshTriangles = new List<int>[2];

        for (int i = 0; i < 2; i++)
        {
            partitionMeshVerts[i] = new List<Vector3>();
            partitionMeshNormals[i] = new List<Vector3>();
            paritionMeshTriangles[i] = new List<int>();
            partitionCapIndexes[i] = new List<int>();
            sameSideDirectVertexMapping[i] = new Dictionary<int, int>();
        }

        // NOTE probably assume we only have one submesh for now
        for (int i = 0; i < mesh.triangles.Length / 3; i++)
        {
            // Cases
            // 1) triangles have all points one partition
            // 2) triangles have 1 point in one parition and 2 in the other
            HashSet<int> triVertsSmallerSubset = new HashSet<int>();
            HashSet<int> triVertsLargerSubset = new HashSet<int>();
            int smallerSubsetPartitionIdx = TOP_PARTION_IDX;
            int largerSubsetParitionIdx = BOTTOM_PARTION_IDX;

            // this code is mega inefficient at the cost of readability for now
            List<int> fullTriangle = new List<int>();
            for (int j = 0; j < 3; j++)
            {
                var vertIdx = mesh.triangles[3 * i + j];
                fullTriangle.Add(vertIdx);
                if (signedVertDistAlongCutNormal[vertIdx] > 0)
                {
                    triVertsSmallerSubset.Add(vertIdx);
                }
                else
                {
                    triVertsLargerSubset.Add(vertIdx);
                }
            }

            if (triVertsSmallerSubset.Count > triVertsLargerSubset.Count)
            {

                (triVertsSmallerSubset, triVertsLargerSubset) = (triVertsLargerSubset, triVertsSmallerSubset);
                (smallerSubsetPartitionIdx, largerSubsetParitionIdx) = (largerSubsetParitionIdx, smallerSubsetPartitionIdx);
            }

            if (triVertsSmallerSubset.Count == 0)
            {
                // ez case
                // add the verts to the direct lookup table if they don't exist
                // and create the new triangle
                var partitionVerts = partitionMeshVerts[largerSubsetParitionIdx];
                var partitionNormals = partitionMeshNormals[largerSubsetParitionIdx];
                var partitionTriangles = paritionMeshTriangles[largerSubsetParitionIdx];
                var sameSideVertMapping = sameSideDirectVertexMapping[largerSubsetParitionIdx];
                foreach (var vertIdx in fullTriangle)
                {
                    if (!sameSideVertMapping.ContainsKey(vertIdx))
                    {
                        partitionVerts.Add(mesh.vertices[vertIdx]);
                        partitionNormals.Add(mesh.normals[vertIdx]);
                        sameSideVertMapping.Add(vertIdx, partitionVerts.Count - 1);
                    }
                    partitionTriangles.Add(sameSideVertMapping[vertIdx]);
                }
                // we are done with this triangle
                continue;
            }

            // tough case we need to break the triangle apart
            // there should be 2 verts on one side and 1 on the other

            // deal with the 1 vert case first
            // creates one new triangle
            // TODO if we are smarter I think we can roll this and the second case together
            // using the technique from the second case
            {
                var smallPartitionVerts = partitionMeshVerts[smallerSubsetPartitionIdx];
                var smallPartitionNormals = partitionMeshNormals[smallerSubsetPartitionIdx];

                var sameSideVertMapping = sameSideDirectVertexMapping[smallerSubsetPartitionIdx];
                var partitionTriangles = paritionMeshTriangles[smallerSubsetPartitionIdx];
                var partitionCaps = partitionCapIndexes[smallerSubsetPartitionIdx];

                Dictionary<int, int> smallVertMappingForTri = new Dictionary<int, int>();
                var edgeStartIdx = triVertsSmallerSubset.ToList()[0];
                var edgeStartVert = mesh.vertices[edgeStartIdx];
                var edgeStartNormal = mesh.normals[edgeStartIdx];

                // this is a direct vert mapping
                if (!sameSideVertMapping.ContainsKey(edgeStartIdx))
                {
                    smallPartitionVerts.Add(edgeStartVert);
                    smallPartitionNormals.Add(edgeStartNormal);
                    sameSideVertMapping.Add(edgeStartIdx, smallPartitionVerts.Count - 1);
                }
                int edgeStartMappedIdx = sameSideVertMapping[edgeStartIdx];
                smallVertMappingForTri.Add(edgeStartIdx, edgeStartMappedIdx);


                // figure out the projected verts
                foreach (var edgeEndIdx in triVertsLargerSubset)
                {
                    var edgeEndVert = mesh.vertices[edgeEndIdx];
                    var edgeRayTorwardsPlane = edgeEndVert - edgeStartVert;

                    var projectedVert = clampEdgeAtPlane(edgeStartVert, edgeRayTorwardsPlane.normalized, cutPlaneNormal, signedVertDistAlongCutNormal[edgeStartIdx]);
                    smallPartitionVerts.Add(projectedVert);
                    // in a really sick implementation this would interpolate the normal along the edge
                    // but for now I'm being lazy and assuming the normals will be consistent across the face
                    var projectedNormal = mesh.normals[edgeEndIdx];
                    smallPartitionNormals.Add(projectedNormal);

                    smallVertMappingForTri.Add(edgeEndIdx, smallPartitionVerts.Count - 1);
                    partitionCaps.Add(smallPartitionVerts.Count - 1);
                }

                foreach (var vertIdx in fullTriangle)
                {
                    partitionTriangles.Add(smallVertMappingForTri[vertIdx]);
                }
            }

            // deal with the 2 vert case
            // creates two new triangles
            {
                var largePartitionVerts = partitionMeshVerts[largerSubsetParitionIdx];
                var largePartitionNormals = partitionMeshNormals[largerSubsetParitionIdx];

                var sameSideVertMapping = sameSideDirectVertexMapping[largerSubsetParitionIdx];
                var partitionTriangles = paritionMeshTriangles[largerSubsetParitionIdx];
                var partitionCaps = partitionCapIndexes[largerSubsetParitionIdx];

                List<int> quadVertIndexes = new List<int>();

                for (int j = 0; j < 3; j++)
                {
                    var prevVertexIdx = fullTriangle[(j - 1 + 3) % 3];
                    var currVertexIdx = fullTriangle[j];
                    var currVertex = mesh.vertices[currVertexIdx];
                    var currNormal = mesh.normals[currVertexIdx];
                    if (triVertsLargerSubset.Contains(currVertexIdx) && !triVertsLargerSubset.Contains(prevVertexIdx))
                    {
                        var prevVert = mesh.vertices[prevVertexIdx];
                        var prevNormal = mesh.normals[prevVertexIdx];
                        var edgeRayTorwardsPlane = prevVert - currVertex;
                        // project the other side vert
                        var projectedVert = clampEdgeAtPlane(currVertex, edgeRayTorwardsPlane.normalized, cutPlaneNormal, signedVertDistAlongCutNormal[currVertexIdx]);
                        largePartitionVerts.Add(projectedVert);
                        // in a really sick implementation this would interpolate the normal along the edge
                        // but for now I'm being lazy and assuming the normals will be consistent across the face
                        var projectedNormal = prevNormal;
                        largePartitionNormals.Add(projectedNormal);
                        quadVertIndexes.Add(largePartitionVerts.Count - 1);
                        partitionCaps.Add(largePartitionVerts.Count - 1);
                    }
                    else if (!triVertsLargerSubset.Contains(currVertexIdx) && triVertsLargerSubset.Contains(prevVertexIdx))
                    {
                        var prevVert = mesh.vertices[prevVertexIdx];
                        var edgeRayTorwardsPlane = currVertex - prevVert;
                        // project the other side vert
                        var projectedVert = clampEdgeAtPlane(prevVert, edgeRayTorwardsPlane.normalized, cutPlaneNormal, signedVertDistAlongCutNormal[prevVertexIdx]);
                        largePartitionVerts.Add(projectedVert);
                        // in a really sick implementation this would interpolate the normal along the edge
                        // but for now I'm being lazy and assuming the normals will be consistent across the face
                        var projectedNormal = currNormal;
                        largePartitionNormals.Add(projectedNormal);
                        quadVertIndexes.Add(largePartitionVerts.Count - 1);
                        partitionCaps.Add(largePartitionVerts.Count - 1);
                    }
                    else if (!triVertsLargerSubset.Contains(currVertexIdx) && !triVertsLargerSubset.Contains(prevVertexIdx))
                    {
                        throw new Exception("Could not decompose triangle");
                    }

                    // seperately
                    // add the same side verts to the direct mapping
                    if (triVertsLargerSubset.Contains(currVertexIdx))
                    {
                        // this is a direct vert mapping
                        if (!sameSideVertMapping.ContainsKey(currVertexIdx))
                        {
                            largePartitionVerts.Add(currVertex);
                            largePartitionNormals.Add(currNormal);
                            sameSideVertMapping.Add(currVertexIdx, largePartitionVerts.Count - 1);
                        }
                        var currVertexMappedIdx = sameSideVertMapping[currVertexIdx];
                        quadVertIndexes.Add(currVertexMappedIdx);
                    }
                }
                partitionTriangles.Add(quadVertIndexes[0]);
                partitionTriangles.Add(quadVertIndexes[1]);
                partitionTriangles.Add(quadVertIndexes[3]);

                partitionTriangles.Add(quadVertIndexes[1]);
                partitionTriangles.Add(quadVertIndexes[2]);
                partitionTriangles.Add(quadVertIndexes[3]);
            }
        }

        // once we establish the cuts we can figure out if they are full cuts or not
        // for a convex mesh at least one of the cuts must fully lie inside the anchor bounds
        for (int i = 0; i < 2; i++)
        {
            Vector3 anchorPoint = anchorPoints[i];
            Vector3 originToAnchor = anchorPoint - cameraPosition;
            Vector3 anchorNormal = Vector3.Cross(cutPlaneNormal, originToAnchor) * (i == 0 ? -1 : 1);
            anchorNormal.Normalize();
            anchorBoundNormals[i] = anchorNormal;
        }

        bool atLeastOneParitionInBounds = false;
        for (int partitionIdx = 0; partitionIdx < 2; partitionIdx++)
        {
            var allPartitionVertsInBounds = true;
            var partitionVerts = partitionMeshVerts[partitionIdx];
            foreach (var vert in partitionVerts)
            {
                for (int i = 0; i < 2; i++)
                {
                    var vertCenteredOnStart = vert - anchorPoints[i];
                    // anchor normals point inwards
                    float signedProj = Vector3.Dot(vertCenteredOnStart, anchorBoundNormals[i]);
                    if (signedProj < 0)
                    {
                        allPartitionVertsInBounds = false;
                        break;
                    }
                }
                if (allPartitionVertsInBounds == false)
                {
                    break;
                }
            }
            atLeastOneParitionInBounds |= allPartitionVertsInBounds;
        }

        if (partitionMeshVerts[BOTTOM_PARTION_IDX].Count == 0 || partitionMeshVerts[TOP_PARTION_IDX].Count == 0)
        {
            Debug.Log("cannot cut convex polyhedra everything is on a single side");
            return;
        }

        if (!atLeastOneParitionInBounds)
        {
            Debug.Log("cannot cut convex polyhedra neither cut side is fully in bounds");
            return;
        }

        // fill holes
        // note we rely heavily on this mesh being convex
        for (int partitionIdx = 0; partitionIdx < 2; partitionIdx++)
        {
            var normalScalar = partitionIdx == TOP_PARTION_IDX ? 1 : -1;
            var partitionVerts = partitionMeshVerts[partitionIdx];
            var partitionNormals = partitionMeshNormals[partitionIdx];
            var capBoundIndexes = partitionCapIndexes[partitionIdx];
            if (capBoundIndexes.Count < 1)
            {
                Debug.Log("no cap bound verts in partition. This is odd");
                continue;
            }

            List<int> dupeCapIndexes = new List<int>();
            Vector3 center = Vector3.zero;
            foreach (var capBoundIdx in capBoundIndexes)
            {
                Vector3 vert = partitionVerts[capBoundIdx];
                center += vert;

                partitionVerts.Add(vert);
                partitionNormals.Add(cutPlaneNormal * -normalScalar);
                dupeCapIndexes.Add(partitionVerts.Count - 1);
            }
            center /= dupeCapIndexes.Count;
            int firstIdx = dupeCapIndexes[0];
            Vector3 firstVert = partitionVerts[firstIdx];
            Vector3 xAxis = (firstVert - center).normalized;
            Vector3 yAxis = Vector3.Cross(xAxis, cutPlaneNormal * normalScalar);
            dupeCapIndexes.Sort((a, b) =>
            {
                Vector3 centeredAVert = partitionVerts[a] - center;
                Vector3 centeredBVert = partitionVerts[b] - center;
                float aX = Vector3.Dot(centeredAVert, xAxis);
                float aY = Vector3.Dot(centeredAVert, yAxis);
                double aAngle = Math.Atan2(aY, aX);
                float bX = Vector3.Dot(centeredBVert, xAxis);
                float bY = Vector3.Dot(centeredBVert, yAxis);
                double bAngle = Math.Atan2(bY, bX);
                return aAngle.CompareTo(bAngle);
            });

            partitionVerts.Add(center);
            partitionNormals.Add(cutPlaneNormal * -normalScalar);
            int centerIdx = partitionVerts.Count - 1;
            var partitionTriangles = paritionMeshTriangles[partitionIdx];
            for (int i = 0; i < dupeCapIndexes.Count; i++)
            {
                int currIdx = dupeCapIndexes[i];
                int nextIdx = dupeCapIndexes[(i + 1) % dupeCapIndexes.Count];
                partitionTriangles.Add(currIdx);
                partitionTriangles.Add(nextIdx);
                partitionTriangles.Add(centerIdx);
            }
        }

        Mesh topMesh = new Mesh()
        {
            vertices = partitionMeshVerts[TOP_PARTION_IDX].ToArray(),
            normals = partitionMeshNormals[TOP_PARTION_IDX].ToArray(),
            triangles = paritionMeshTriangles[TOP_PARTION_IDX].ToArray(),
        };
        Mesh bottomMesh = new Mesh()
        {
            vertices = partitionMeshVerts[BOTTOM_PARTION_IDX].ToArray(),
            normals = partitionMeshNormals[BOTTOM_PARTION_IDX].ToArray(),
            triangles = paritionMeshTriangles[BOTTOM_PARTION_IDX].ToArray(),
        };

        meshFilter.sharedMesh = topMesh;
        meshCollider.sharedMesh = topMesh;

        var secondSliceable = Instantiate(SliceablePrefab, transform.parent);
        secondSliceable.transform.localPosition = transform.localPosition;
        secondSliceable.transform.localRotation = transform.localRotation;
        secondSliceable.transform.localScale = transform.localScale;
        Physics.SyncTransforms();
        secondSliceable.GetComponent<MeshFilter>().sharedMesh = bottomMesh;
        secondSliceable.GetComponent<MeshCollider>().sharedMesh = bottomMesh;

        // handle screwables
        var screwableBody = GetComponent<ScrewableBody>();
        if (screwableBody != null && screwableBody.AttachedScrews.Count > 0)
        {
            var secondScrewableBody = secondSliceable.AddComponent<ScrewableBody>();
            // divide the screws
            // we use a clone here so we can modify in place when screws detach
            foreach (var screw in screwableBody.AttachedScrews.ToList())
            {
                var localScrewPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(screw.transform.position);
                var side = Vector3.Dot(localScrewPosition - startPoint, cutPlaneNormal);
                if (side <= 0)
                {
                    // in this case we are on the bottom mesh and need to reassign
                    screw.SwapAttachedBody(screwableBody, secondScrewableBody);
                }
            }
        }
    }

    private Vector3 clampEdgeAtPlane(Vector3 edgeStart, Vector3 normalEdgeRayThroughPlane, Vector3 planeNormal, float signedStartShortestDistToPlane)
    {
        var cosBetweenRayAndDown = Vector3.Dot(normalEdgeRayThroughPlane, planeNormal);
        var amountToExtendRay = -signedStartShortestDistToPlane / cosBetweenRayAndDown;
        return edgeStart + normalEdgeRayThroughPlane * amountToExtendRay;
    }
}