using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CutTool : BaseTool
{
    private Vector3 _startCutPoint;
    private Vector3 _endCutPoint;
    private bool _clicking;

    public float MaxSelectRange;

    public LineRenderer SelectedLineRender;

    public Sliceable SelectedCuttable;
    public Vector3 LocalSliceableCutOrigin;
    public Vector3 LocalSliceableCutNormal;
    public Vector3 LocalSliceableCutDirection;
    public Plane SlicePlane;

    public override void ActiveToolUpdate(Camera camera)
    {
        var cameraTransform = camera.transform;
        LineRenderer _lineRenderer = GetComponent<LineRenderer>();
        if (Input.GetMouseButtonDown(0))
        {
            if (SelectedCuttable == null)
            {
                bool didHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, MaxSelectRange);
                if (didHit && hit.distance < MaxSelectRange && hit.collider != null)
                {
                    var sliceable = hit.collider.gameObject.GetComponent<Sliceable>();
                    if (sliceable != null)
                    {
                        SelectedCuttable = sliceable;
                        var highlightable = sliceable.gameObject.GetComponent<Highlightable>();
                        LocalSliceableCutNormal = sliceable.transform.worldToLocalMatrix.MultiplyVector(hit.normal);
                        LocalSliceableCutOrigin = sliceable.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                        SlicePlane = new Plane(LocalSliceableCutNormal, LocalSliceableCutOrigin);
                        if (highlightable != null)
                        {
                            highlightable.Select();
                        }
                    }
                }
            }
            else
            {
                // look for intersection point with the plane
                var eyeRay = new Ray(camera.transform.position, camera.transform.forward);
                float distance;
                if (SlicePlane.Raycast(eyeRay, out distance))
                {

                }
            }
        }
    }

    public override string GetName()
    {
        return "Cut Tool";
    }

    public void OnDrawGizmos()
    {
        if (SelectedCuttable)
        {
            var origin = SelectedCuttable.transform.localToWorldMatrix.MultiplyPoint(LocalSliceableCutOrigin);
            var normal = SelectedCuttable.transform.localToWorldMatrix.MultiplyVector(LocalSliceableCutNormal);
            Gizmos.DrawLine(origin, origin + normal * 0.5f);
        }
    }
}
