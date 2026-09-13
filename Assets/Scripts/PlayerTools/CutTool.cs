using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CutTool : BaseTool
{
    private Vector3 _startCutPoint;
    private Vector3 _endCutPoint;
    private bool _clicking;

    public float MaxCutRange;

    public LineRenderer SelectedLineRender;

    public override void ActiveToolUpdate(Camera camera)
    {
        var cameraTransform = camera.transform;
        LineRenderer _lineRenderer = GetComponent<LineRenderer>();
        if (!_clicking)
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool didHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, MaxCutRange);
                float distance = MaxCutRange;
                if (didHit)
                {
                    distance = hit.distance;
                }
                _startCutPoint = cameraTransform.position + cameraTransform.forward * distance;
                _clicking = true;
            }
        }
        else
        {
            bool didHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, MaxCutRange);
            float distance = MaxCutRange;
            if (didHit)
            {
                distance = hit.distance;
            }
            _endCutPoint = cameraTransform.position + cameraTransform.forward * distance;
            // we are going to cast to all colliders the question is what are we going to do with them
            Vector3 sphereCenter = (cameraTransform.position + _startCutPoint + _endCutPoint) / 3;
            float sphereRadius = Mathf.Max(
                (cameraTransform.position - sphereCenter).magnitude,
                (_startCutPoint - sphereCenter).magnitude,
                (_endCutPoint - sphereCenter).magnitude);
            var colliders = Physics.OverlapSphere(sphereCenter, sphereRadius);


            if (Input.GetMouseButton(0))
            {
                Debug.Log("clickking");
                bool validSliceFound = false;
                Vector3 earliestSliceableSectionStart = Vector3.zero;
                Vector3 earliestSliceableSectionEnd = Vector3.zero;
                foreach (var col in colliders)
                {
                    var sliceable = col.gameObject.GetComponent<Sliceable>();
                    if (sliceable != null)
                    {
                        var res = sliceable.GetSliceableSection(cameraTransform.position, _startCutPoint, _endCutPoint, 1000, camera);
                        if (res.canSlice)
                        {
                            Debug.Log("can slice");
                        }
                        if (res.canSlice)
                        {
                            validSliceFound = true;
                            earliestSliceableSectionStart = res.start;
                            earliestSliceableSectionEnd = res.end;
                        }
                    }
                }
                if (validSliceFound)
                {
                    SelectedLineRender.SetPositions(new Vector3[] { earliestSliceableSectionStart, earliestSliceableSectionEnd });
                    SelectedLineRender.positionCount = 2;
                }
                else
                {
                    SelectedLineRender.positionCount = 0;
                }
            }
            else
            {
                _clicking = false;
                // cut logic
                Debug.Log("cut");
                // TODO obviously this needs to be improved
                // TODO create a thin box here an find the correct orientation to narrow down sliceables
                // Then for each sliceable that's possible get enter and exit points
                // Then determine for each pair of enter and exit points (a segment) if they are allowed to cut
                // by either construction a mesh collider or using raycasting along the segment
                foreach (var col in colliders)
                {
                    var sliceable = col.gameObject.GetComponent<Sliceable>();
                    if (sliceable != null)
                    {
                        sliceable.Slice(cameraTransform.position, _startCutPoint, _endCutPoint, 1000, camera);
                    }
                }
            }
        }

        if (_clicking)
        {
            Debug.Log("updato:" + _startCutPoint + "," + _endCutPoint);
            _lineRenderer.SetPositions(new Vector3[] { _startCutPoint, _endCutPoint });
            _lineRenderer.positionCount = 2;
            // from: https://www.reddit.com/r/Unity2D/comments/kt01nv/dotted_linerenderer_fixed/
            // b/c I am lazy
            _lineRenderer.material.mainTextureScale = new Vector2(1f / _lineRenderer.startWidth, 1.0f);
        }
        else
        {
            _lineRenderer.SetPositions(new Vector3[] { });
            _lineRenderer.positionCount = 0;
            SelectedLineRender.positionCount = 0;
        }
    }

    public override string GetName()
    {
        return "Cut Tool";
    }
}
