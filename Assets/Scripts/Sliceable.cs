using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Sliceable : MonoBehaviour
{
    public void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshCollider = GetComponent<MeshCollider>();
        if (meshFilter.sharedMesh != meshCollider.sharedMesh)
        {
            throw new System.Exception("meshFilter sharedMesh does not equal meshCollider sharedMesh");
        }
        if (!meshCollider.convex)
        {
            throw new System.Exception("mesh must be convex (the note collider being convex is just a proxy for this)");
        }
    }

    private void ParitionVerticies()
    {

    }

    public void Slice(Vector3 cameraPosition, Vector3 startPoint, Vector3 endPoint, float maxSliceRange)
    {
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
            Vector3 anchorNormal = Vector3.Cross(cutPlaneNormal, originToAnchor);
            anchorNormal.Normalize();
            anchorBoundNormals[i] = anchorNormal;
        }

        // now we assign vertices to a side or out of bounds
        // if there are out of bounds verts on both sides refuse to cut I think
        // assuming the mesh is convex then that means the cut wasn't all the way through?

        var meshFilter = GetComponent<MeshFilter>();
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
                Vector3 vertFromAnchorPoint = vert - anchorPoints[i];
                float signedDistanceAlongBoundNormal = Vector3.Dot(vertFromAnchorPoint, anchorBoundNormals[i]);
                // Note I'm totally guessing about the side of the bounds
                // Worst case we just check the middle but I think we can vibe it
                if (signedDistanceAlongBoundNormal > 0)
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

            for (int j = 0; j < 3; j++)
            {
                var vertIdx = mesh.triangles[3 * i + j];
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
                var partitionTriangles = paritionMeshTriangles[largerSubsetParitionIdx];
                var sameSideVertMapping = sameSideDirectVertexMapping[largerSubsetParitionIdx];
                foreach (var vertIdx in triVertsLargerSubset)
                {
                    if (!sameSideVertMapping.ContainsKey(vertIdx))
                    {
                        partitionVerts.Add(mesh.vertices[vertIdx]);
                        sameSideVertMapping.Add(vertIdx, partitionVerts.Count - 1);
                    }
                    partitionTriangles.Add(sameSideVertMapping[vertIdx]);
                }
            }
        }
    }
}