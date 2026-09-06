using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScrewableBody : MonoBehaviour
{
    private static int _nextScrewableBodyId = 0;

    public HashSet<Screw> AttachedScrews = new HashSet<Screw>();
    public int Id = -1;

    // -1 means dirty
    private int _smallestAttachedBodyId = -1;

    private static SortedDictionary<int, ScrewableBody> AllBodies = new SortedDictionary<int, ScrewableBody>();

    private static GameObject ScrewableBodyRoot;

    private float _volume = -1.0f;

    public float density = 1.0f;

    public static void InitScrewableBodySystem(GameObject root)
    {
        ScrewableBodyRoot = root;
    }

    public void MarkAttachedIslandDirty()
    {
        MarkAttachedIslandWithSmallestBodyId(-1);
    }

    public float GetMeshVolume()
    {
        if (_volume < 0)
        {
            RefreshMeshVolume();
        }
        return _volume;
    }

    public void ReparentBodyAndToggleRigidBody()
    {
        var parentBody = GetComponentInParent<ScrewableBody>();
        int parentBodyId;
        if (parentBody == null)
        {
            // you are your own parent if no one above you is
            parentBodyId = Id;
        }
        else
        {
            parentBodyId = parentBody.Id;
        }

        var rb = gameObject.GetComponent<Rigidbody>();
        if (_smallestAttachedBodyId == Id)
        {
            if (rb == null)
            {
                var rb2 = gameObject.AddComponent<Rigidbody>();
                rb2.interpolation = RigidbodyInterpolation.Interpolate;
                rb2.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                gameObject.AddComponent<Draggable>();
            }
        }
        else if (rb != null)
        {
            Destroy(GetComponent<Draggable>());
            Destroy(rb);
        }

        if (_smallestAttachedBodyId == Id && transform.parent != ScrewableBodyRoot.transform)
        {
            transform.SetParent(ScrewableBodyRoot.transform, true);
        }

        // do not re parent on being marked dirty
        if (_smallestAttachedBodyId >= 0 && _smallestAttachedBodyId != parentBodyId)
        {
            Transform newParentTransform;
            if (_smallestAttachedBodyId == Id)
            {
                newParentTransform = ScrewableBodyRoot.transform;
            }
            else
            {
                newParentTransform = AllBodies[_smallestAttachedBodyId].transform;
            }
            transform.SetParent(newParentTransform, true);
        }
    }

    public void MarkAttachedIslandWithSmallestBodyId(int smallestIslandBodyId)
    {
        var islandBodies = new Queue<ScrewableBody>();
        islandBodies.Enqueue(this);
        while (islandBodies.Count > 0)
        {
            ScrewableBody islandBody = islandBodies.Dequeue();
            if (islandBody._smallestAttachedBodyId == smallestIslandBodyId)
            {
                // we already marked it
                continue;
            }
            else
            {
                // mark it
                islandBody._smallestAttachedBodyId = smallestIslandBodyId;

                // consider re parenting the body if it's not just being marked dirty
                if (smallestIslandBodyId >= 0)
                {
                    islandBody.ReparentBodyAndToggleRigidBody();
                }

                foreach (var screw in islandBody.AttachedScrews)
                {
                    foreach (var body in screw.AttachedBodies)
                    {
                        islandBodies.Enqueue(body);
                    }
                }
            }
        }
    }

    public static void RefreshDirtyBodyHierarchy()
    {
        foreach (var idBodyPair in AllBodies)
        {
            var initalIslandBody = idBodyPair.Value;

            // if you are not dirty skip
            var dirty = initalIslandBody._smallestAttachedBodyId < 0;
            if (!dirty)
            {
                continue;
            }

            // assume we have not marked this island yet
            // also since we are sorted by key going lowest to highest we must
            // be the smallest key on the island
            var smallestIslandBodyId = idBodyPair.Key;
            initalIslandBody.MarkAttachedIslandWithSmallestBodyId(smallestIslandBodyId);
        }
        foreach (var idBodyPair in AllBodies)
        {
            idBodyPair.Value.RefreshRigidBodyMass();
        }
    }
    public void RefreshMeshVolume()
    {
        var collider = GetComponent<Collider>();
        if (collider.GetType() == typeof(MeshCollider))
        {
            var meshCollider = collider as MeshCollider;
            _volume = MeshUtils.VolumeOfMesh(meshCollider.sharedMesh, transform.localToWorldMatrix);
        }
        else if (collider.GetType() == typeof(BoxCollider))
        {
            var boxCollider = collider as BoxCollider;
            var worldSpaceColliderSize = transform.localToWorldMatrix.MultiplyPoint3x4(boxCollider.size);
            _volume = worldSpaceColliderSize.x * worldSpaceColliderSize.y * worldSpaceColliderSize.z;
        }
        else
        {
            throw new System.Exception("Cannot calculate the mass of collider type " + collider.name);
        }
    }

    public float GetMeshMass()
    {
        return GetMeshVolume() * density;
    }

    private float GetRigidBodyMassAccountingForChildren()
    {
        float mass = 0.0f;
        foreach (var body in gameObject.GetComponentsInChildren<ScrewableBody>())
        {
            mass += body.GetMeshMass();
        }
        return mass;
    }


    private void Awake()
    {
        Id = IncrementId();
        AllBodies.Add(Id, this);
        RefreshMeshVolume();
    }

    public int IncrementId()
    {
        return _nextScrewableBodyId++;
    }

    public void RefreshRigidBodyMass()
    {
        if (_smallestAttachedBodyId == Id)
        {
            var rb = GetComponent<Rigidbody>();
            rb.mass = GetRigidBodyMassAccountingForChildren();
        }
    }

    public Rigidbody GetRigidbody()
    {
        return GetComponent<Rigidbody>();
    }
}
