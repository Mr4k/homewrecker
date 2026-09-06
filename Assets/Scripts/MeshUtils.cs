using System;
using UnityEngine;

public class MeshUtils
{
    public static float VolumeOfMesh(Mesh mesh, Matrix4x4 localToWorldMatrix)
    {
        float signedVolume = 0.0f;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v1Idx = mesh.triangles[i];
            var v2Idx = mesh.triangles[i + 1];
            var v3Idx = mesh.triangles[i + 2];
            var v1 = mesh.vertices[v1Idx];
            var v2 = mesh.vertices[v2Idx];
            var v3 = mesh.vertices[v3Idx];
            signedVolume += SignedVolumeOfTriangle(
                localToWorldMatrix.MultiplyPoint3x4(v1),
                localToWorldMatrix.MultiplyPoint3x4(v2),
                localToWorldMatrix.MultiplyPoint3x4(v3)
            );
        }
        return Math.Abs(signedVolume);
    }

    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return 1.0f / 6.0f * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
}