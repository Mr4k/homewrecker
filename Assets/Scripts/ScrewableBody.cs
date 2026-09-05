using System.Collections.Generic;
using UnityEngine;

public class ScrewableBody : MonoBehaviour
{
    private static int NextScrewableBodyId = 0;

    public HashSet<Screw> AttachedScrews = new HashSet<Screw>();
    public int Id = -1;

    // -1 means dirty
    private int SmallestAttachedBodyId = -1;

    private static SortedDictionary<int, ScrewableBody> AllBodies = new SortedDictionary<int, ScrewableBody>();

    private static GameObject ScrewableBodyRoot;

    public static void InitScrewableBodySystem(GameObject root)
    {
        ScrewableBodyRoot = root;
    }

    public void MarkAttachedIslandDirty()
    {
        MarkAttachedIslandWithSmallestBodyId(-1);
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
        if (SmallestAttachedBodyId == Id)
        {
            if (rb == null)
            {
                gameObject.AddComponent<Draggable>();
                gameObject.AddComponent<Rigidbody>();
            }
        }
        else if (rb != null)
        {
            Destroy(GetComponent<Draggable>());
            Destroy(rb);
        }

        if (SmallestAttachedBodyId == Id && transform.parent != ScrewableBodyRoot.transform)
        {
            transform.SetParent(ScrewableBodyRoot.transform, true);
        }

        // do not re parent on being marked dirty
        if (SmallestAttachedBodyId >= 0 && SmallestAttachedBodyId != parentBodyId)
        {
            Transform newParentTransform;
            if (SmallestAttachedBodyId == Id)
            {
                newParentTransform = ScrewableBodyRoot.transform;
            }
            else
            {
                newParentTransform = AllBodies[SmallestAttachedBodyId].transform;
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
            if (islandBody.SmallestAttachedBodyId == smallestIslandBodyId)
            {
                // we already marked it
                continue;
            }
            else
            {
                // mark it
                islandBody.SmallestAttachedBodyId = smallestIslandBodyId;

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
            var dirty = initalIslandBody.SmallestAttachedBodyId < 0;
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
    }

    private void Awake()
    {
        Id = IncrementId();
        AllBodies.Add(Id, this);
    }

    public int IncrementId()
    {
        return NextScrewableBodyId++;
    }

    public Rigidbody GetRigidbody()
    {
        return GetComponent<Rigidbody>();
    }
}
