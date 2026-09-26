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
    public bool _cutLineEstablished = false;

    private void Deselect()
    {
        _cutLineEstablished = false;
        SelectedCuttable = null;
    }

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
                        // askhelly parenting is dangerous in case the parent gets destoryed then rip our objects
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
                // look for intersection point with the plane
                var eyeRay = new Ray(camera.transform.position, camera.transform.forward);
                float distance;
                if (SlicePlane.Raycast(eyeRay, out distance))
                {
                    CutPupil.transform.position = eyeRay.direction * distance + eyeRay.origin;
                    CutPupil.transform.SetParent(SelectedCuttable.transform, true);
                    _cutLineEstablished = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (SelectedCuttable != null && _cutLineEstablished)
            {
                float r = SelectedCuttable.gameObject.GetComponent<MeshCollider>().bounds.size.magnitude;
                var dir = CutPupil.transform.position - CutEye.transform.position;
                var d = dir.magnitude;
                if (d > 0.1f)
                {
                    Debug.Log("cutttt");
                    // cut
                    var normal = SelectedCuttable.transform.localToWorldMatrix.MultiplyVector(LocalSliceableCutNormal);
                    SelectedCuttable.Slice(
                        CutEye.transform.position + normal,
                        CutEye.transform.position + dir * r,
                        CutEye.transform.position - dir * r,
                        1000,
                        camera
                    );
                }
                Deselect();
            }
        }
        if (SelectedCuttable != null && _cutLineEstablished)
        {
            float r = SelectedCuttable.gameObject.GetComponent<MeshCollider>().bounds.size.magnitude;
            var dir = CutPupil.transform.position - CutEye.transform.position;
            var d = dir.magnitude;
            var maxAlphaMag = 0.125f;
            var minAlphaMag = 0.1f;
            var alpha = Math.Min(Math.Max(d - minAlphaMag, 0) / (maxAlphaMag - minAlphaMag), 1.0f);
            dir.Normalize();
            _lineRenderer.SetPositions(new Vector3[] { CutEye.transform.position + dir * r, CutEye.transform.position - dir * r });
            _lineRenderer.positionCount = 2;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0), new GradientAlphaKey(alpha, 1) }
            );
            _lineRenderer.colorGradient = gradient;
            // from: https://www.reddit.com/r/Unity2D/comments/kt01nv/dotted_linerenderer_fixed/
            // b/c I am lazy
            _lineRenderer.material.mainTextureScale = new Vector2(1f / _lineRenderer.startWidth, 1.0f);
        }
        else
        {
            _lineRenderer.positionCount = 0;
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
