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
    public GameObject CutEye;
    public GameObject CutPupil;

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
                        // Note we should really rebuilt this so it says relevant if the local space shifts
                        SlicePlane = new Plane(
                            sliceable.transform.localToWorldMatrix.MultiplyVector(LocalSliceableCutNormal),
                            sliceable.transform.localToWorldMatrix.MultiplyPoint(LocalSliceableCutOrigin)
                        );

                        CutEye.transform.position = hit.point;
                        CutEye.transform.SetParent(SelectedCuttable.transform, true);

                        if (highlightable != null)
                        {
                            highlightable.Select();
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (SelectedCuttable != null)
            {
                SlicePlane.SetNormalAndPosition(
                    SelectedCuttable.transform.localToWorldMatrix.MultiplyVector(LocalSliceableCutNormal),
                    SelectedCuttable.transform.localToWorldMatrix.MultiplyPoint(LocalSliceableCutOrigin)
                );
                float r = SelectedCuttable.gameObject.GetComponent<MeshCollider>().bounds.size.magnitude;
                // look for intersection point with the plane
                var eyeRay = new Ray(camera.transform.position, camera.transform.forward);
                float distance;
                if (SlicePlane.Raycast(eyeRay, out distance))
                {
                    CutPupil.transform.position = eyeRay.direction * distance + eyeRay.origin;
                    CutPupil.transform.SetParent(SelectedCuttable.transform, true);
                    var dir = CutPupil.transform.position - CutEye.transform.position;
                    dir.Normalize();
                    SelectedLineRender.SetPositions(new Vector3[] { CutEye.transform.position + dir * r, CutEye.transform.position - dir * r });
                    SelectedLineRender.positionCount = 2;
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
