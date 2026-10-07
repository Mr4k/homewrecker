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
                /*else if (Input.GetMouseButtonDown(1))
                {
                    var draggable = hit.rigidbody.GetComponent<Draggable>();
                    draggable.Unteather();
                    if (_held == draggable)
                    {
                        _held = null;
                    }
                }*/

            }
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
        if (state == State.Rotating && _held != null)
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
            //var newLookQuat = Quaternion.LookRotation(new Vector3(cameraTransform.position.x, _held.targetWorldPosition.y, cameraTransform.position.z) - _held.targetWorldPosition);
            //var quat = ShortestPathBetweenTwoQuats(initialLookQuat, newLookQuat);
            //_held.targetWorldRotation = quat * initialRotationUponPickup;
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