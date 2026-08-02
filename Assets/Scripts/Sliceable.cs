using System;
using System.Collections.Generic;
using System.Linq;
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
        for (int i = 0; i < 2; i++)
        {
            Vector3 anchorPoint = anchorPoints[i];
            Vector3 originToAnchor = anchorPoint - cameraPosition;
            Vector3 anchorNormal = Vector3.Cross(cutPlaneNormal, originToAnchor) * (i == 0 ? -1 : 1);
            anchorNormal.Normalize();
            anchorBoundNormals[i] = anchorNormal;
        }

        // now we assign vertices to a side or out of bounds
        // if there are out of bounds verts on both sides refuse to cut I think
        // assuming the mesh is convex then that means the cut wasn't all the way through?

        var meshFilter = GetComponent<MeshFilter>();
        var meshCollider = GetComponent<MeshCollider>();
        var mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Dictionary<int, float> signedVertDistAlongCutNormal = new Dictionary<int, float>();
        bool topSideAllInBounds = true;
        bool bottomSideAllInBounds = true;
        for (var i = 0; i < vertices.Length; i++)
        {
            Vector3 vert = vertices[i];
            // figure out which side of the cut plane we are on
            Vector3 vertFromStart = vert - startPoint;
            float signedDistanceAlongCutNormal = Vector3.Dot(vertFromStart, cutPlaneNormal);
            bool isTop = signedDistanceAlongCutNormal < 0;
            // then do a bounds check
            bool isInBounds = true;
            for (int j = 0; j < 2; j++)
            {
                // Note at least one of these is redundant
                Vector3 vertFromAnchorPoint = vert - anchorPoints[j];
                float signedDistanceAlongBoundNormal = Vector3.Dot(vertFromAnchorPoint, anchorBoundNormals[j]);
                // Note I'm totally guessing about the side of the bounds
                // Worst case we just check the middle but I think we can vibe it
                if (signedDistanceAlongBoundNormal < 0)
                {
                    isInBounds = false;
                    break;
                }
            }
            signedVertDistAlongCutNormal.Add(i, signedDistanceAlongCutNormal);
            if (isTop)
            {
                if (!isInBounds)
                {
                    topSideAllInBounds = false;
                }
            }
            else
            {
                if (!isInBounds)
                {
                    bottomSideAllInBounds = false;
                }
            }
        }

        /*if (vertsAboveByIdx.Count > 0 && vertsBelowByIdx.Count > 0)
        {
            Debug.Log("all verts on one side cannot cut polyhedra");
            return;
        }*/
        // maybe not totally sufficient
        if (!bottomSideAllInBounds && !topSideAllInBounds)
        {
            Debug.Log("both top and bottom are out of bounds polyheadra cannot be cut");
            return;
        }

        // cut the mesh
        // broken verts will form the faces we need to construct
        // uhhh maybe caps seem a little complicated
        // feels like one way is to take a string and then wind it around the polygon on the plane

        const int TOP_PARTION_IDX = 0;
        const int BOTTOM_PARTION_IDX = 1;
        List<Vector3>[] paritionMeshVerts = new List<Vector3>[2];
        List<Vector3>[] paritionMeshNormals = new List<Vector3>[2];
        Dictionary<int, int>[] sameSideDirectVertexMapping = new Dictionary<int, int>[2];
        List<int>[] paritionMeshTriangles = new List<int>[2];

        for (int i = 0; i < 2; i++)
        {
            paritionMeshVerts[i] = new List<Vector3>();
            paritionMeshNormals[i] = new List<Vector3>();
            paritionMeshTriangles[i] = new List<int>();
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
                var partitionVerts = paritionMeshVerts[largerSubsetParitionIdx];
                var partitionNormals = paritionMeshNormals[largerSubsetParitionIdx];
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
            {
                var smallPartitionVerts = paritionMeshVerts[smallerSubsetPartitionIdx];
                var smallPartitionNormals = paritionMeshNormals[smallerSubsetPartitionIdx];

                var sameSideVertMapping = sameSideDirectVertexMapping[smallerSubsetPartitionIdx];
                var partitionTriangles = paritionMeshTriangles[smallerSubsetPartitionIdx];

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
                }

                foreach (var vertIdx in fullTriangle)
                {
                    partitionTriangles.Add(smallVertMappingForTri[vertIdx]);
                }
            }

            // deal with the 2 vert case
            // creates two new triangles
            {
                var largePartitionVerts = paritionMeshVerts[largerSubsetParitionIdx];
                var largePartitionNormals = paritionMeshNormals[largerSubsetParitionIdx];

                var sameSideVertMapping = sameSideDirectVertexMapping[largerSubsetParitionIdx];
                var partitionTriangles = paritionMeshTriangles[largerSubsetParitionIdx];

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
                    }
                    else if (!triVertsLargerSubset.Contains(currVertexIdx) && triVertsLargerSubset.Contains(prevVertexIdx))
                    {
                        var prevVert = mesh.vertices[prevVertexIdx];
                        var prevNormal = mesh.normals[prevVertexIdx];
                        var edgeRayTorwardsPlane = currVertex - prevVert;
                        // project the other side vert
                        var projectedVert = clampEdgeAtPlane(prevVert, edgeRayTorwardsPlane.normalized, cutPlaneNormal, signedVertDistAlongCutNormal[prevVertexIdx]);
                        largePartitionVerts.Add(projectedVert);
                        // in a really sick implementation this would interpolate the normal along the edge
                        // but for now I'm being lazy and assuming the normals will be consistent across the face
                        var projectedNormal = currNormal;
                        largePartitionNormals.Add(projectedNormal);
                        quadVertIndexes.Add(largePartitionVerts.Count - 1);
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

        Mesh topMesh = new Mesh()
        {
            vertices = paritionMeshVerts[TOP_PARTION_IDX].ToArray(),
            normals = paritionMeshNormals[TOP_PARTION_IDX].ToArray(),
            triangles = paritionMeshTriangles[TOP_PARTION_IDX].ToArray(),
        };
        Mesh bottomMesh = new Mesh()
        {
            vertices = paritionMeshVerts[BOTTOM_PARTION_IDX].ToArray(),
            normals = paritionMeshNormals[BOTTOM_PARTION_IDX].ToArray(),
            triangles = paritionMeshTriangles[BOTTOM_PARTION_IDX].ToArray(),
        };

        meshFilter.sharedMesh = topMesh;
        meshCollider.sharedMesh = topMesh;

        var secondSliceable = Instantiate(SliceablePrefab, transform.parent);
        secondSliceable.transform.localPosition = transform.localPosition;
        secondSliceable.transform.localRotation = transform.localRotation;
        secondSliceable.transform.localScale = transform.localScale;
        secondSliceable.GetComponent<MeshFilter>().sharedMesh = bottomMesh;
        secondSliceable.GetComponent<MeshCollider>().sharedMesh = bottomMesh;
    }

    private Vector3 clampEdgeAtPlane(Vector3 edgeStart, Vector3 normalEdgeRayThroughPlane, Vector3 planeNormal, float signedStartShortestDistToPlane)
    {
        var cosBetweenRayAndDown = Vector3.Dot(normalEdgeRayThroughPlane, planeNormal);
        var amountToExtendRay = -signedStartShortestDistToPlane / cosBetweenRayAndDown;
        return edgeStart + normalEdgeRayThroughPlane * amountToExtendRay;
    }
}