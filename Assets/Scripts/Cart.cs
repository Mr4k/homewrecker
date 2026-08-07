using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cart : Draggable
{
    private Wheel[] _wheels;

    public bool ShouldStablize = true;

    public float SmallUprightStabilizationSpring = 20f;
    public float BigUprightSpringConstant = 100f;

    public float maxRotAnglePerSec = 1.0f;

    public void Start()
    {
        _wheels = GetComponentsInChildren<Wheel>();
    }

    public override void BeginDrag()
    {
        foreach (var w in _wheels)
        {
            w.dragged = true;
        }
        base.BeginDrag();
    }

    public override void OnDrag(Vector3 dragDirection)
    {
        foreach (var w in _wheels)
        {
            w.pullDirection = dragDirection;
        }
        base.OnDrag(dragDirection);
    }

    public override void EndDrag()
    {
        foreach (var w in _wheels)
        {
            w.pullDirection = Vector3.zero;
            w.dragged = false;
        }
        base.EndDrag();
    }

    public virtual void FixedUpdate()
    {
        var body = GetComponent<Rigidbody>();
        // body.AddTorque(Vector3.Cross(transform.up, Vector3.up) * SmallUprightStabilizationSpring);
        if (ShouldStablize)
        {
            body.AddTorque(Vector3.Cross(transform.up, Vector3.up) * BigUprightSpringConstant);
        }
    }
}
