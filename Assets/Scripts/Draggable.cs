using System;
using UnityEngine;
using UnityEngine.Animations;

enum DragState
{
    None,
    Suspended,
    Translating,
    RotatingX,
    RotatingY,
    RotatingZ,
}

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    public Vector3 targetWorldPosition;
    public Quaternion targetWorldRotation;
    public float maxStabilizationAcceleration = 10;
    public float maxGoalDiffVelChange = 10;
    public float maxRotationCorrectionAngularVelChange = 10;
    private DragState state;
    private Quaternion targetRotation;

    int numCollisions = 0;

    private void FixedUpdate()
    {
        var _rigidbody = GetComponent<Rigidbody>();
        switch (state)
        {
            case DragState.Translating:
                _rigidbody.isKinematic = false;
                _rigidbody.constraints = RigidbodyConstraints.None;
                _rigidbody.freezeRotation = true;
                _rigidbody.useGravity = false;

                var goalDiff = targetWorldPosition - _rigidbody.position;
                var goalAxis = goalDiff.normalized;
                float goalProj = Vector3.Dot(_rigidbody.linearVelocity, goalAxis);
                Vector3 extraVel = _rigidbody.linearVelocity - goalAxis * goalProj;
                // remove extra vel if possible
                Vector3 opposingExtraForce = -extraVel;
                opposingExtraForce = opposingExtraForce.normalized * Math.Min(opposingExtraForce.magnitude, maxStabilizationAcceleration);
                _rigidbody.AddForce(opposingExtraForce, ForceMode.VelocityChange);

                // push motion along goal axis toward goal
                var goalVel = goalAxis * goalProj;
                var totalGoalVelChange = goalDiff.magnitude * goalAxis * 10 - goalVel;
                totalGoalVelChange = totalGoalVelChange.normalized * Math.Min(totalGoalVelChange.magnitude, maxGoalDiffVelChange);
                _rigidbody.AddForce(totalGoalVelChange, ForceMode.VelocityChange);
                var it = _rigidbody.inertiaTensor;
                it.x = 1;
                it.y = 1;
                it.z = 1;
                _rigidbody.inertiaTensor = it;
                break;
            case DragState.Suspended:
                _rigidbody.isKinematic = true;
                _rigidbody.constraints = RigidbodyConstraints.None;
                _rigidbody.freezeRotation = false;
                _rigidbody.useGravity = true;
                break;
            case DragState.None:
                _rigidbody.isKinematic = false;
                _rigidbody.constraints = RigidbodyConstraints.None;
                _rigidbody.useGravity = true;
                _rigidbody.freezeRotation = true;
                break;
            case DragState.RotatingX:
                // rotation
                var delta = targetWorldRotation * Quaternion.Inverse(_rigidbody.rotation);
                delta.ToAngleAxis(out float angDeg, out Vector3 axis);
                float angRad = angDeg * Mathf.Deg2Rad;
                Vector3 torque = axis.normalized * (angRad * 5) - _rigidbody.angularVelocity * 0.8f;
                _rigidbody.AddTorque(torque, ForceMode.VelocityChange);
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.freezeRotation = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                foreach (var comp in GetComponentsInChildren<Collider>())
                {
                    comp.isTrigger = true;
                }
                break;
            case DragState.RotatingY:
                // rotation
                delta = targetWorldRotation * Quaternion.Inverse(_rigidbody.rotation);
                delta.ToAngleAxis(out angDeg, out axis);
                angRad = angDeg * Mathf.Deg2Rad;
                torque = axis.normalized * (angRad * 5) - _rigidbody.angularVelocity * 0.8f;
                _rigidbody.AddTorque(torque, ForceMode.VelocityChange);
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.freezeRotation = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                foreach (var comp in GetComponentsInChildren<Collider>())
                {
                    comp.isTrigger = true;
                }
                break;
            case DragState.RotatingZ:
                // rotation
                delta = targetWorldRotation * Quaternion.Inverse(_rigidbody.rotation);
                delta.ToAngleAxis(out angDeg, out axis);
                angRad = angDeg * Mathf.Deg2Rad;
                torque = axis.normalized * (angRad * 5) - _rigidbody.angularVelocity * 0.8f;
                _rigidbody.AddTorque(torque, ForceMode.VelocityChange);
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.freezeRotation = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                foreach (var comp in GetComponentsInChildren<Collider>())
                {
                    comp.isTrigger = true;
                }
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        numCollisions += 1;
    }

    void OnCollisionExit(Collision collision)
    {
        numCollisions -= 1;
    }

    protected void LateUpdate()
    {
        /*switch (state)
        {
            case DragState.RotatingX:
                var _rigidbody = GetComponent<Rigidbody>();
                _rigidbody.angularVelocity = Vector3.Project(_rigidbody.angularVelocity, transform.right);
                break;
            case DragState.RotatingY:
                _rigidbody = GetComponent<Rigidbody>();
                _rigidbody.angularVelocity = Vector3.Project(_rigidbody.angularVelocity, transform.up);
                break;
            case DragState.RotatingZ:
                _rigidbody = GetComponent<Rigidbody>();
                _rigidbody.angularVelocity = Vector3.Project(_rigidbody.angularVelocity, transform.forward);
                break;
        }*/
    }

    public void EnableRotation(Gimbal.RotationAxisName localAxis)
    {
        switch (localAxis)
        {
            case Gimbal.RotationAxisName.X:
                state = DragState.RotatingX;
                break;
            case Gimbal.RotationAxisName.Y:
                state = DragState.RotatingY;
                break;
            case Gimbal.RotationAxisName.Z:
                state = DragState.RotatingZ;
                break;
        }
    }

    public void DisableRotation()
    {
        state = DragState.Suspended;
    }

    public virtual void Pickup()
    {
        state = DragState.Translating;
    }

    public void Suspend()
    {
        targetWorldPosition = transform.position;
        targetWorldRotation = transform.rotation;
        state = DragState.Suspended;
    }

    public virtual void Release()
    {
        state = DragState.None;
    }
}
