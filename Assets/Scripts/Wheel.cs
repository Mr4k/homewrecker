using System;
using UnityEngine;

// stolen and modified from https://github.com/unity-car-tutorials/SimpleRaycastVehicle-Unity/blob/master/Assets/Scripts/RaycastWheelSimple.cs
// b/c I'm lazy

public class Wheel : MonoBehaviour
{
    public Rigidbody parent;
    public float radius;
    public float maxSuspension;
    public float spring = 100.0f;
    public float damper = 0.5f;
    public float skidFriction = 0.9f;
    // this exists b/c we cast from inside a collidable but the spring should starts lower
    // TODO eliminate this it's more a vibe hack that a real thing with a physics engine basis
    public float suspensionOffset = 0.05f;
    public Vector3 pullDirection = Vector3.zero;
    public bool dragged = false;
    public void FixedUpdate()
    {
        Vector3 localDownward = transform.TransformDirection(-Vector3.up);
        Vector3 downwards = -Vector3.up;
        //Vector3 downwards = -Vector3.up;
        RaycastHit hit;
        float distFactor = Math.Max(Vector3.Dot(localDownward, downwards) - 0.75f, 0) / 0.25f;
        float distanceToCast = (radius + maxSuspension) * distFactor;

        var collider = GetComponent<Collider>();
        if (dragged)
        {
            collider.enabled = false;
        }
        else
        {
            collider.enabled = true;
        }

        if (dragged && Physics.SphereCast(transform.position, 0.001f, downwards, out hit, distanceToCast))
        {
            // the velocity at point of contact
            Vector3 velocityAtTouch = parent.GetPointVelocity(hit.point);

            // calculate spring compression
            // difference in positions divided by total suspension range
            float compression = Math.Max(hit.distance - suspensionOffset, 0) / (maxSuspension + radius);
            compression = -compression + 1;

            // final force
            Vector3 force = -downwards * compression * spring;
            // velocity at point of contact transformed into local space

            Vector3 t = transform.InverseTransformDirection(velocityAtTouch);

            // local x and z directions = 0
            t.z = t.x = 0;

            // back to world space * -damping
            Vector3 damping = transform.TransformDirection(t) * -damper;

            Vector3 frictionForce;
            if (dragged)
            {
                Vector3 skidAxis = Vector3.Cross(-downwards, pullDirection).normalized;
                Vector3 velAlongSkid = Vector3.Dot(velocityAtTouch, skidAxis) * skidAxis;
                frictionForce = -velAlongSkid * skidFriction;
            }
            else
            {
                Vector3 velOnPlane = transform.InverseTransformDirection(velocityAtTouch);
                velOnPlane.y = 0;
                velOnPlane = transform.TransformDirection(velOnPlane);
                frictionForce = -velOnPlane * skidFriction;
            }
            Vector3 finalForce = force + damping + frictionForce;

            parent.AddForceAtPosition(finalForce, hit.point);
        }
    }
}