using UnityEngine;

[ExecuteInEditMode]
public class MeshHub : MonoBehaviour
{
    private static MeshHub _hub;

    public Mesh CubeMesh;

    private void Update()
    {
        _hub = this;
    }

    public static Mesh GetCubeMesh()
    {
        return _hub.CubeMesh;
    }
}