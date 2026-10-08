using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class HandTool : BaseTool
{
    public enum State
    {
        Grabbing,
        Rotating,
    }
    // for grabber
    public float GrabRange = 5f;
    public Draggable _held;
    public Vector3 _heldGrabOffsetInHeldLocalSpace;

    // where should the held object try to be on the xz plane
    public float currXZPlaneTargetAngle = 0;

    // how far the object is from you on the xz place (unsigned)
    public float currHeldFlatDistance = 0;
    private State state = State.Rotating;

    //public Quaternion initialRotationUponPickup;
    //public Quaternion initialLookQuat;

    private Nullable<Gimbal.RotationAxisName> axisSelected;
    private Vector3 axisSelectedOriginPoint;
    private Vector3 axisSelectedWorldTangent;
    private Vector3 axisToRotateAround;

    public LayerMask GimbalMask;

    public Quaternion ShortestPathBetweenTwoQuats(Quaternion b, Quaternion a)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(Multiply(b, -1));
        }
        else
            return a * Quaternion.Inverse(b);
    }

    // this is used for when objects are split or unscrewed
    public void SetAsHeldUp(Draggable draggable)
    {
        draggable.targetWorldPosition = draggable.transform.position;
        draggable.targetWorldRotation = draggable.transform.rotation;
        draggable.Suspend();
    }

    public static Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public override void ActiveToolUpdate(Camera camera)
    {
        Transform cameraTransform = camera.transform;
        if (_held == null)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, GrabRange))
            {
                if (hit.rigidbody && hit.rigidbody.GetComponent<Draggable>())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        var draggable = hit.rigidbody.GetComponent<Draggable>();
                        _held = draggable;
                        _held.Pickup();
                        _held.targetWorldRotation = _held.transform.rotation;
                        var offset = _held.transform.position - hit.point;
                        _heldGrabOffsetInHeldLocalSpace = _held.transform.worldToLocalMatrix.MultiplyVector(offset);
                        _held.targetWorldPosition = hit.point;
                        var projectedFlatDistance = _held.targetWorldPosition - cameraTransform.position;
                        projectedFlatDistance.y = 0;
                        currHeldFlatDistance = projectedFlatDistance.magnitude;
                    }
                }
            }
        }
        if (_held != null && state == State.Rotating && axisSelected == null)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit2, GrabRange, GimbalMask))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var tangentData = Gimbal.Singleton.GetTangent(hit2.collider, hit2.point);
                    axisSelected = tangentData.LocalAxis;
                    axisSelectedOriginPoint = tangentData.WorldSpacePoint;
                    axisSelectedWorldTangent = tangentData.WorldSpaceTangent;
                    axisToRotateAround = tangentData.WorldSpaceNormal;
                    _held.EnableRotation(tangentData.LocalAxis);
                    Debug.Log("chose axis " + axisSelected);
                }
            }
        }
        if (Input.GetMouseButton(0) && state == State.Rotating && _held != null && axisSelected != null)
        {
            //Input.mousePositionDelta
            Vector3 p1 = camera.WorldToScreenPoint(axisSelectedOriginPoint);
            Vector3 p2 = camera.WorldToScreenPoint(axisSelectedOriginPoint + axisSelectedWorldTangent);
            Vector3 normalizedScreenSpaceTangentDirection = (p2 - p1).normalized;
            float deltaProjection = Vector3.Dot(Input.mousePositionDelta, normalizedScreenSpaceTangentDirection);
            Debug.Log("delta projection:" + deltaProjection);
            _held.targetWorldRotation = Quaternion.AngleAxis(deltaProjection * 2, axisToRotateAround) * _held.targetWorldRotation;
        }
        if (Input.GetMouseButtonUp(0) && state == State.Rotating && _held != null && axisSelected != null)
        {
            axisSelected = null;
            _held.DisableRotation();
        }
        if (Keyboard.current.zKey.isPressed && _held != null)
        {
            _held.Suspend();
            _held = null;
        }
        if (Input.GetMouseButtonDown(1) && _held != null)
        {
            _held.Release();
            _held = null;
        }
        if (state == State.Rotating && axisSelected != null)
        {
            Gimbal.Singleton.transform.rotation = _held.transform.rotation;
        }
        if (state == State.Rotating && _held != null && axisSelected == null)
        {
            _held.Suspend();
            Gimbal.Singleton.transform.position = _held.transform.position;
        }

        if (Keyboard.current.xKey.isPressed)
        {
            if (state == State.Grabbing)
            {
                state = State.Rotating;
            }
            else
            {
                state = State.Grabbing;
                axisSelected = null;
            }
        }

        if (_held != null)
        {
            // note there is a singularity when the player looks straight up
            // our simple response is to just ban them from doing it (looking 100% straight up)
            currXZPlaneTargetAngle = (float)Math.Atan2(camera.transform.forward.z, camera.transform.forward.x);
            var _heldOffsetInWorldSpace = _held.transform.localToWorldMatrix.MultiplyVector(_heldGrabOffsetInHeldLocalSpace);
            var sinTheta = camera.transform.forward.y / cameraTransform.forward.magnitude;
            var cosTheta = Math.Sqrt(1 - sinTheta * sinTheta);
            var hyp = (float)(currHeldFlatDistance / cosTheta);
            _held.targetWorldPosition = cameraTransform.position + new Vector3(Mathf.Cos(currXZPlaneTargetAngle), 0, Mathf.Sin(currXZPlaneTargetAngle)) * currHeldFlatDistance + Vector3.up * (sinTheta * hyp) + _heldOffsetInWorldSpace;
        }
    }

    public override void ActiveToolFixedUpdate(FirstPersonCharacterController character)
    {
    }

    public override void ToolDeselected()
    {
        if (_held != null)
        {
            _held = null;
        }
        base.ToolDeselected();
    }
    public override string GetName()
    {
        return "Grabber";
    }
}