using System;
using System.Collections.Generic;
using UnityEngine;

public class HandTool : BaseTool
{
    // for grabber
    public float GrabRange = 5f;
    public Transform PullTarget;
    public Draggable _held;
    public HashSet<Draggable> _allHeld = new HashSet<Draggable>();
    public Vector3 _heldGrabOffsetInHeldLocalSpace;

    // where should the held object try to be on the xz plane
    public float currXZPlaneTargetAngle = 0;

    // how far the object is from you on the xz place (unsigned)
    public float currHeldFlatDistance = 0;

    // how far the object is from you on the y axis (signed)
    public float currHeldFloatDistance = 0;

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
                    if (!_allHeld.Contains(_held))
                    {
                        _held.Pickup();
                        _allHeld.Add(_held);
                    }
                    var worldPoint = cameraTransform.position + cameraTransform.forward;
                    var offset = _held.transform.position - worldPoint;
                    _heldGrabOffsetInHeldLocalSpace = _held.transform.worldToLocalMatrix.MultiplyPoint(offset);
                    _held.targetWorldRotation = _held.transform.rotation;
                    _held.targetWorldPosition = _held.transform.position - worldPoint;
                }
            }
        }
        if (!Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(1))
        {
            if (_held != null)
            {
                _allHeld.Remove(_held);
                _held.Drop();
            }
        }
        if (_held != null)
        {
            var cameraDirXZProj = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
            var cameraDirYProj = camera.transform.forward.y * ;
            var targetWorldPoint = camera.transform.forward;
            var _heldOffsetInWorldSpace = _held.transform.worldToLocalMatrix.MultiplyPoint(_heldGrabOffsetInHeldLocalSpace);
            _held.targetWorldPosition = _held.transform.position - worldPoint;
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