using System;
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
    public float Damping = 8f;
    public Draggable _held;

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

        Draggable target = _held;
        if (target == null && Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, GrabRange))
            target = hit.rigidbody ? hit.rigidbody.GetComponent<Draggable>() : null;
        if (Input.GetMouseButtonDown(0))
        {
            _held = target;
        }
        else if (!Input.GetMouseButton(0))
        {
            _held = null;
        }
    }

    private void FixedUpdate()
    {
        float fixedDeltaTimeMul = Time.fixedDeltaTime * 60;
        if (_held != null)
        {
            var targetDisplacement = PullTarget.position - _held.transform.position;
            var relativeVelocity = _held.Rigidbody.linearVelocity - Character.Motor.Velocity;
            var force = targetDisplacement * 100 - relativeVelocity * 10;
            var normalizedForce = force.normalized;
            var magForce = force.magnitude;
            magForce = Math.Min(magForce, PullForce);
            _held.Rigidbody.AddForce(normalizedForce * magForce * fixedDeltaTimeMul);
        }
    }
}
