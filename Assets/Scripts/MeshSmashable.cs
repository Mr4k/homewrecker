using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeshSmashable : Smashable
{
    public override void Smash(Vector3 originPoint, float smashForce)
    {
        var debrisGameObject = new GameObject
        {
            name = "MeshDebris" + UnityEngine.Random.Range(0, 100)
        };
        debrisGameObject.transform.SetParent(transform.parent, false);
        var debris = debrisGameObject.AddComponent<Rigidbody>();
        debris.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        debris.interpolation = RigidbodyInterpolation.Interpolate;


        debris.transform.localScale = transform.localScale;
        debris.transform.localPosition = transform.localPosition;

        var meshFilter = debrisGameObject.AddComponent<MeshFilter>();
        var currMesh = GetComponent<MeshFilter>().mesh;
        meshFilter.mesh = currMesh;

        MeshRenderer meshRenderer = debrisGameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        MeshCollider meshCollider = debrisGameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.sharedMesh = currMesh;

        Vector3 force = (transform.position - originPoint) * smashForce;
        debris.AddForce(force);
        Destroy(this.gameObject);
    }
}
