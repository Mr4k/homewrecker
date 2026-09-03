using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    public float GizmoDisplayRadius = 0.05f;
    private float GizmoScrewLength = 0.25f;

    public ScrewableBody[] AttachedBodies = new ScrewableBody[2];

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
        throw new System.Exception("TODO!");
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.DrawRay(transform.position, transform.forward * GizmoScrewLength);
    }
}