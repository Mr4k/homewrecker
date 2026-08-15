using System;
using UnityEngine;

public class HandTool : BaseTool
{
    // for grabber
    public float GrabRange = 5f;
    public Transform PullTarget;
    public float PullForce = 60f;
    public float StablizerTorque = 10f;
    public float Damping = 8f;
    public Draggable _held;
    public Vector3 _heldGrabPoint;
    public override void ActiveToolUpdate(Transform cameraTransform)
    {
        if (_held == null && Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, GrabRange))
        {
            if (hit.rigidbody && hit.rigidbody.GetComponent<Draggable>())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var draggable = hit.rigidbody.GetComponent<Draggable>();
                    _held = draggable;
                    _heldGrabPoint = _held.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                    _held.BeginDrag();
                }
            }
        }
        if (_held != null && !Input.GetMouseButton(0))
        {
            _held.EndDrag();
            _held = null;
        }
    }

    public override void ActiveToolFixedUpdate(FirstPersonCharacterController character)
    {
        float fixedDeltaTimeMul = Time.fixedDeltaTime * 60;
        if (_held != null)
        {
            var heldGrabPointWorld = _held.transform.localToWorldMatrix.MultiplyPoint(_heldGrabPoint);
            var targetDisplacement = PullTarget.position - heldGrabPointWorld;
            var relativeVelocity = _held.Rigidbody.linearVelocity - character.Motor.Velocity;
            var force = targetDisplacement * 100 - relativeVelocity * 10;
            var normalizedForce = force.normalized;
            var magForce = force.magnitude;
            magForce = Math.Min(magForce, PullForce);
            // note that surfing is a bug. Should we keep it? Could be fun
            // to combat surfing maybe we just make it so that you cannot pull something inside yourself
            _held.Rigidbody.AddForceAtPosition(normalizedForce * magForce * fixedDeltaTimeMul, heldGrabPointWorld);
            _held.Rigidbody.AddTorque(-_held.Rigidbody.angularVelocity * 0.1f * fixedDeltaTimeMul);
            _held.OnDrag(normalizedForce);
        }
        base.ActiveToolFixedUpdate(character);
    }

    public override void ToolDeselected()
    {
        if (_held != null)
        {
            _held.EndDrag();
            _held = null;
        }
        base.ToolDeselected();
    }
    public override string GetName()
    {
        return "Grabber";
    }
}