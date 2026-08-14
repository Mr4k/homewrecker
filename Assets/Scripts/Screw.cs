using UnityEngine;

public class Screw : MonoBehaviour
{
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public FixedJoint ScrewJoint;

    public void UnScrew()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.DrawRay(transform.position, transform.forward * GizmoScrewLength);
    }
}