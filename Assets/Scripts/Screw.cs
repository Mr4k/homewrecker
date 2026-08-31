using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    public static float ScrewJointBreakForce = 300;
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public Joint ScrewJoint;

    public ScrewableBody ParentBody;
    public ScrewableBody AttachedBody;

    public void Start()
    {
        ParentBody.AttachedScrews.Add(this);
        AttachedBody.AttachedScrews.Add(this);
    }

    public void Unscrew()
    {
        ScrewJoint.enableCollision = true;
        Destroy(ScrewJoint);
        ParentBody.AttachedScrews.Remove(this);
        AttachedBody.AttachedScrews.Remove(this);
        Destroy(gameObject);
    }

    // Note for now this just Inits the joint
    public void Init()
    {
        ScrewJoint = ParentBody.AddComponent<FixedJoint>();
        //ScrewJoint.enableCollision = true;
        ScrewJoint.connectedBody = AttachedBody.GetRigidbody();
        ScrewJoint.breakForce = ScrewJointBreakForce;
        ScrewJoint.breakTorque = ScrewJointBreakForce;
        ScrewJoint.enablePreprocessing = false;
    }

    public void SwapAttachedBody(ScrewableBody oldBody, ScrewableBody newBody)
    {
        if (oldBody == ParentBody)
        {
            ParentBody.AttachedScrews.Remove(this);
            Destroy(ScrewJoint);
            ParentBody = newBody;
            this.transform.SetParent(ParentBody.transform, true);
            ParentBody.AttachedScrews.Add(this);
            Init();
        }
        else if (oldBody == AttachedBody)
        {
            AttachedBody.AttachedScrews.Remove(this);
            Destroy(ScrewJoint);
            AttachedBody = newBody;
            AttachedBody.AttachedScrews.Add(this);
            Init();
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