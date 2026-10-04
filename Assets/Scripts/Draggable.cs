using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    private bool _dragged;
    public Vector3 targetWorldPosition;
    public Quaternion targetWorldRotation;
    public float maxStabilizationAcceleration = 2;
    public float maxGoalDiffVelChange = 4;
    public float maxRotationCorrectionAngularVelChange = 5;
    private void FixedUpdate()
    {
        if (_dragged)
        {
            var _rigidbody = GetComponent<Rigidbody>();
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

            // rotation
            _rigidbody.AddTorque(-_rigidbody.angularVelocity + (Vector3.Cross(transform.up, targetWorldRotation * Vector3.up) + Vector3.Cross(transform.right, targetWorldRotation * Vector3.right)) * maxRotationCorrectionAngularVelChange, ForceMode.VelocityChange);
        }
    }

    public virtual void Pickup()
    {
        _dragged = true;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
    }

    public virtual void Drop()
    {
        _dragged = false;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = true;
    }
}
