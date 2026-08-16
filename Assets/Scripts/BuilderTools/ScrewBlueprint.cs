using UnityEngine;

public class ScrewBlueprint : BaseBlueprint
{
    public ScrewableBody Body1;
    public ScrewableBody Body2;
    public float GizmoDisplayRadius = 0.05f;
    public float ScrewLength = 0.25f;
    public float ScrewDiameter = 0.1f;
    public GameObject ScrewPrefab;

    public Screw ActiveScrew;
    public override void RefreshBlueprint()
    {
        if (ActiveScrew != null)
        {
            DestroyImmediate(ActiveScrew.ScrewJoint);
            DestroyImmediate(ActiveScrew.gameObject);
        }

        var screwGameObject = Instantiate(ScrewPrefab);
        screwGameObject.transform.position = transform.position;
        screwGameObject.transform.rotation = transform.rotation;
        var screw = screwGameObject.GetComponent<Screw>();
        screwGameObject.transform.localScale = new Vector3(ScrewDiameter, ScrewDiameter, ScrewLength);
        screw.ScrewJoint = Body1.gameObject.AddComponent<FixedJoint>();
        screw.ScrewJoint.connectedBody = Body2.GetRigidbody();
        screw.ScrewJoint.enableCollision = true;
        screw.ParentBody = Body1;
        screw.AttachedBody = Body2;
        screwGameObject.transform.SetParent(Body1.transform, true);
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
