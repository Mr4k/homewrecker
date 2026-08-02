using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class FirstPersonPlayer : MonoBehaviour
{
    public FirstPersonCharacterController Character;
    public Transform CameraTransform;
    public float LookSensitivity = 2f;
    private float _pitch;

    // for grabber
    public float GrabRange = 5f;
    public Transform PullTarget;
    public float PullForce = 60f;
    public float StablizerTorque = 10f;
    public float Damping = 8f;
    public Draggable _held;
    public Vector3 _heldGrabPoint;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;

        Vector2 look = Cursor.lockState == CursorLockMode.Locked
            ? new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * LookSensitivity
            : Vector2.zero;

        FirstPersonInputs inputs = new FirstPersonInputs
        {
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            LookYaw = look.x,
            Jump = Input.GetKeyDown(KeyCode.Space),
        };
        Character.SetInputs(ref inputs);

        _pitch = Mathf.Clamp(_pitch - look.y, -89f, 89f);
        CameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        if (_held == null && Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, GrabRange))
        {
            if (hit.rigidbody && hit.rigidbody.GetComponent<Draggable>())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var draggable = hit.rigidbody.GetComponent<Draggable>();
                    _held = draggable;
                    _heldGrabPoint = _held.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                }
            }
            else if (hit.collider && hit.collider.GetComponent<Smashable>())
            {
                var smashableGroupObjects = new List<Smashable>();
                var smashable = hit.collider.GetComponent<Smashable>();
                smashableGroupObjects.Add(smashable);
                var colliders = Physics.OverlapSphere(hit.point, 0.25f);
                foreach (var col in colliders)
                {
                    var otherSmashable = col.gameObject.GetComponent<Smashable>();
                    if (otherSmashable != null && otherSmashable != smashable && otherSmashable.SharesGroup(smashable))
                    {
                        smashableGroupObjects.Add(otherSmashable);
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("smashhhed!");
                    foreach (var s in smashableGroupObjects)
                    {
                        s.Smash(CameraTransform.position, 400);
                    }
                }
            }
        }
        if (_held != null && !Input.GetMouseButton(0))
        {
            _held = null;
        }
        // TODO this line should be generalized with all other player tools 
        // and let you select the active one
        GetComponent<CutTool>().ActiveToolUpdate(CameraTransform);
    }

    private void FixedUpdate()
    {
        float fixedDeltaTimeMul = Time.fixedDeltaTime * 60;
        if (_held != null)
        {
            var heldGrabPointWorld = _held.transform.localToWorldMatrix.MultiplyPoint(_heldGrabPoint);
            var targetDisplacement = PullTarget.position - heldGrabPointWorld;
            var relativeVelocity = _held.Rigidbody.linearVelocity - Character.Motor.Velocity;
            var force = targetDisplacement * 100 - relativeVelocity * 10;
            var normalizedForce = force.normalized;
            var magForce = force.magnitude;
            magForce = Math.Min(magForce, PullForce);
            // note that surfing is a bug. Should we keep it? Could be fun
            // to combat surfing maybe we just make it so that you cannot pull something inside yourself
            _held.Rigidbody.AddForceAtPosition(normalizedForce * magForce * fixedDeltaTimeMul, heldGrabPointWorld);
            _held.Rigidbody.AddTorque(-_held.Rigidbody.angularVelocity * 0.1f * fixedDeltaTimeMul);
        }
    }
}
