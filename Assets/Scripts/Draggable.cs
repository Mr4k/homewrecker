using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    private bool _dragged;
    public Vector3 targetWorldPosition;
    public Quaternion targetWorldRotation;
    public PIDParameters positionControllerParams;
    public PIDController[] positionControllers = new PIDController[3];


    private void FixedUpdate()
    {
        if (_dragged)
        {
            var _rigidbody = GetComponent<Rigidbody>();
            var vel = Vector3.Lerp(targetWorldPosition - _rigidbody.position, Vector3.zero, 0.8f);
            vel = vel.normalized * Math.Min(vel.magnitude, 0.5f);
            int i = 0;
            float epsilon = 0.01f;
            while (vel.magnitude > epsilon && i < 3)
            {
                Vector3 velToMoveThisStep = vel;
                bool velRemaining = false;
                {
                    RaycastHit hitInfo;
                    if (_rigidbody.SweepTest(vel.normalized, out hitInfo, vel.magnitude))
                    {
                        velRemaining = true;
                        var targetPos = _rigidbody.position + vel.normalized * Math.Max(hitInfo.distance - epsilon, 0);
                        velToMoveThisStep = targetPos - _rigidbody.position;
                        Vector3 remainingVel = vel.normalized * Math.Max(vel.magnitude - hitInfo.distance - epsilon, 0);
                        vel = Vector3.ProjectOnPlane(remainingVel, hitInfo.normal);
                    }
                }
                _rigidbody.MovePosition(_rigidbody.position + velToMoveThisStep);
                if (!velRemaining)
                {
                    break;
                }
                i++;
            }

        }
    }

    public virtual void Pickup()
    {
        _dragged = true;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
    }

    public virtual void Drop()
    {
        _dragged = false;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = false;
    }
}
