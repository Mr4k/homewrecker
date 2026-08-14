using UnityEngine;

public class ScrewBlueprint : BaseBlueprint
{
    public Rigidbody FirstBody;
    public Rigidbody SecondBody;
    public float GizmoDisplayRadius = 0.05f;
    public float ScrewLength = 0.25f;

    public Screw ActiveScrew;
    public override void RefreshBlueprint()
    {
        if (ActiveScrew != null)
        {
            DestroyImmediate(ActiveScrew.ScrewJoint);
            DestroyImmediate(ActiveScrew.gameObject);
        }

        var screwGameObject = new GameObject
        {
            name = "Screw"
        };
        screwGameObject.transform.position = transform.position;
        screwGameObject.transform.rotation = transform.rotation;
        var screw = screwGameObject.AddComponent<Screw>();
        screw.ScrewJoint = FirstBody.gameObject.AddComponent<FixedJoint>();
        screw.ScrewJoint.connectedBody = SecondBody;
        screwGameObject.transform.parent = FirstBody.transform;
        ActiveScrew = screw;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, GizmoDisplayRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * ScrewLength);
    }
}
