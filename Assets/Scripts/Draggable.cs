using System;
using UnityEngine;

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
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezeRotationX;
                _rigidbody.freezeRotation = false;
                break;
            case DragState.RotatingY:
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezeRotationY;
                _rigidbody.freezeRotation = false;
                break;
            case DragState.RotatingZ:
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezeRotationZ;
                _rigidbody.freezeRotation = false;
                break;
        }
    }

    public virtual void Pickup()
    {
        state = DragState.Translating;
    }

    public void Suspend()
    {
        targetWorldPosition = transform.position;
        state = DragState.Suspended;
    }

    public virtual void Release()
    {
        state = DragState.None;
    }
}
