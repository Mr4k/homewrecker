using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public ScrewableBody[] AttachedBodies = new ScrewableBody[2];
    public Vector3 _intersectionPositionRelativeToParentBody = Vector3.zero;

    public void Init(ScrewableBody body1, ScrewableBody body2, Vector3 worldIntersectionPosition)
    {
        AttachedBodies[0] = body1;
        AttachedBodies[1] = body2;
        transform.SetParent(AttachedBodies[0].transform, true);
        _intersectionPositionRelativeToParentBody = AttachedBodies[0].transform.worldToLocalMatrix.MultiplyPoint3x4(worldIntersectionPosition);
    }

    public void Start()
    {
        foreach (var body in AttachedBodies)
        {
            body.AttachedScrews.Add(this);
        }
    }

    public Vector3 getWorldIntersectionPosition()
    {
        return AttachedBodies[0].transform.localToWorldMatrix.MultiplyPoint3x4(_intersectionPositionRelativeToParentBody);
    }

    public void Unscrew()
    {
        foreach (var body in AttachedBodies)
        {
            body.MarkAttachedIslandDirty();
        }
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
        var worldIntersectionPosition = getWorldIntersectionPosition();
        for (int i = 0; i < AttachedBodies.Length; i++)
        {
            if (AttachedBodies[i] == oldBody)
            {
                AttachedBodies[i] = newBody;
            }
        }
        transform.SetParent(AttachedBodies[0].transform, true);
        _intersectionPositionRelativeToParentBody = AttachedBodies[0].transform.worldToLocalMatrix.MultiplyPoint3x4(worldIntersectionPosition);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(getWorldIntersectionPosition(), GizmoDisplayRadius * 0.5f);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.DrawRay(transform.position, transform.forward * GizmoScrewLength);
        Gizmos.DrawSphere(getWorldIntersectionPosition(), GizmoDisplayRadius * 0.5f);
    }
}