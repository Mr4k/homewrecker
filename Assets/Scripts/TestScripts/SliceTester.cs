using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SliceTester : MonoBehaviour
{
    public Sliceable testSubject;
    public Camera sliceCamera;
    public Vector3 sliceStart;
    public Vector3 sliceEnd;

    private bool _hasSliced;

    public void Update()
    {
        var lr = GetComponent<LineRenderer>();
        lr.SetPositions(new Vector3[2] { sliceStart, sliceEnd });
        if (!_hasSliced)
        {
            testSubject.Slice(sliceCamera.transform.position, sliceStart, sliceEnd, 1000);
            _hasSliced = true;
        }
    }
}