using UnityEngine;

public class GameObjectUtils
{
    public static Mesh ForkAndScaleBaseMesh(Mesh baseMesh, Vector3 scale)
    {
        var newMesh = new Mesh();
        newMesh.name = baseMesh.name + "_Scaled";

        Vector3[] vertices = newMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], scale);
        }
        newMesh.vertices = vertices;

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        return newMesh;
    }
}