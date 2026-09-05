using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public ScrewableBody[] AttachedBodies = new ScrewableBody[2];

    public void Init(ScrewableBody body1, ScrewableBody body2)
    {
        AttachedBodies[0] = body1;
        AttachedBodies[1] = body2;
    }

    public void Start()
    {
        foreach (var body in AttachedBodies)
        {
            body.AttachedScrews.Add(this);
        }
    }

    public void Unscrew()
    {
        foreach (var body in AttachedBodies)
        {
            body.AttachedScrews.Remove(this);
        }
        Destroy(gameObject);
    }

    public void SwapAttachedBody(ScrewableBody oldBody, ScrewableBody newBody)
    {
        oldBody.MarkAttachedIslandDirty();
        newBody.MarkAttachedIslandDirty();
        oldBody.AttachedScrews.Remove(this);
        newBody.AttachedScrews.Add(this);
        for (int i = 0; i < AttachedBodies.Length; i++)
        {
            if (AttachedBodies[i] == oldBody)
            {
                AttachedBodies[i] = newBody;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.DrawRay(transform.position, transform.forward * GizmoScrewLength);
    }
}