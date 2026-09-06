using Unity.Mathematics;
using UnityEngine;

public class ScrewBlueprint : BaseBlueprint
{
    public ScrewableBody Body1;
    public float GizmoDisplayRadius = 0.05f;
    public float ScrewLength = 0.25f;
    public float ScrewDiameter = 0.1f;
    public GameObject ScrewPrefab;

    public Screw ActiveScrew;
    public override void RefreshBlueprint()
    {
        if (ActiveScrew != null)
        {
            DestroyImmediate(ActiveScrew.gameObject);
        }

        var screwGameObject = Instantiate(ScrewPrefab);
        screwGameObject.transform.position = transform.position;
        screwGameObject.transform.rotation = transform.rotation;
        var screw = screwGameObject.GetComponent<Screw>();
        screwGameObject.transform.localScale = new Vector3(ScrewDiameter, ScrewDiameter, ScrewLength);

        var results = Physics.RaycastAll(new Ray(transform.position, transform.forward), ScrewLength);
        var lowestDistance = ScrewLength * 10;
        ScrewableBody body2 = null;
        Vector3 worldIntersectionPoint = Vector3.zero;
        foreach (var res in results)
        {
            var screwableBody2 = res.collider.gameObject.GetComponent<ScrewableBody>();
            if (screwableBody2 && screwableBody2 != Body1)
            {
                if (res.distance < lowestDistance)
                {
                    lowestDistance = res.distance;
                    body2 = res.collider.gameObject.GetComponent<ScrewableBody>();
                    worldIntersectionPoint = res.point;
                }
            }

        }
        if (body2 == null)
        {
            throw new System.Exception("Could not screw in screw could not find other body");
        }

        screw.Init(Body1, body2, worldIntersectionPoint);
        ActiveScrew = screw;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, ScrewDiameter / 2.0f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * ScrewLength);
    }
}
