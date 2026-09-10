using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CutTool : BaseTool
{
    private Vector3 _startCutPoint;
    private Vector3 _endCutPoint;
    private bool _clicking;

    public float MaxCutRange;

    public override void ActiveToolUpdate(Transform cameraTransform)
    {
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
            if (Input.GetMouseButton(0))
            {
                Debug.Log("clickking");
                bool didHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, MaxCutRange);
                float distance = MaxCutRange;
                if (didHit)
                {
                    distance = hit.distance;
                }
                _endCutPoint = cameraTransform.position + cameraTransform.forward * distance;
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
                Vector3 sphereCenter = (cameraTransform.position + _startCutPoint + _endCutPoint) / 3;
                float sphereRadius = Mathf.Max(
                    (cameraTransform.position - sphereCenter).magnitude,
                    (_startCutPoint - sphereCenter).magnitude,
                    (_endCutPoint - sphereCenter).magnitude);
                var colliders = Physics.OverlapSphere(sphereCenter, sphereRadius);
                foreach (var col in colliders)
                {
                    var sliceable = col.gameObject.GetComponent<Sliceable>();
                    if (sliceable != null)
                    {
                        sliceable.Slice(cameraTransform.position, _startCutPoint, _endCutPoint, 1000);
                    }
                }
            }
        }

        if (_clicking)
        {
            Debug.Log("updato:" + _startCutPoint + "," + _endCutPoint);
            _lineRenderer.SetPositions(new Vector3[] { _startCutPoint, _endCutPoint });
            _lineRenderer.positionCount = 2;
        }
        else
        {
            _lineRenderer.SetPositions(new Vector3[] { });
            _lineRenderer.positionCount = 0;
        }
    }

    public override string GetName()
    {
        return "Cut Tool";
    }
}
