using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CutTool : MonoBehaviour
{
    private Vector3 _startCutPoint;
    private Vector3 _endCutPoint;
    private bool _clicking;

    public float MaxCutRange;

    public void ActiveToolUpdate(Transform cameraTransform)
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
                foreach (var slicable in FindObjectsByType<Sliceable>(FindObjectsSortMode.None))
                {
                    slicable.Slice(cameraTransform.position, _startCutPoint, _endCutPoint, 1000);
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
}
