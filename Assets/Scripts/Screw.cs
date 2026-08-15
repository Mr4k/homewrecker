using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public FixedJoint ScrewJoint;

    public ScrewableBody ParentBody;
    public ScrewableBody AttachedBody;

    public void Start()
    {
        ParentBody.AttachedScrews.Add(this);
        AttachedBody.AttachedScrews.Add(this);
    }

    public void UnScrew()
    {
        ParentBody.AttachedScrews.Remove(this);
        AttachedBody.AttachedScrews.Remove(this);
    }

    public void SwapAttachedBody(ScrewableBody oldBody, ScrewableBody newBody)
    {
        if (oldBody == ParentBody)
        {
            ParentBody.AttachedScrews.Remove(this);
            Destroy(ScrewJoint);

            ParentBody = newBody;

            this.transform.parent = ParentBody.transform;
            ParentBody.AttachedScrews.Add(this);
            ScrewJoint = ParentBody.AddComponent<FixedJoint>();
            ScrewJoint.connectedBody = AttachedBody.GetRigidbody();
        }
        else if (oldBody == AttachedBody)
        {
            AttachedBody.AttachedScrews.Remove(this);
            Destroy(ScrewJoint);

            AttachedBody = newBody;

            AttachedBody.AttachedScrews.Add(this);
            ScrewJoint = ParentBody.AddComponent<FixedJoint>();
            ScrewJoint.connectedBody = AttachedBody.GetRigidbody();
        }
        else
        {
            throw new System.Exception("oldBody is neither screw");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.DrawRay(transform.position, transform.forward * GizmoScrewLength);
    }
}