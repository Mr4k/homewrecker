using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cart : Draggable
{
    private Wheel[] _wheels;

    public bool ShouldStablize = true;

    public float UprightSpringConstant = 1.0f;

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
        if (ShouldStablize)
        {
            var body = GetComponent<Rigidbody>();
            var targetRot = Quaternion.FromToRotation(this.transform.up, Vector3.up) * this.transform.rotation;
            var currRot = Quaternion.Slerp(this.transform.rotation, targetRot, 0.1f);
            //var dst = 1.0f - Math.Max(Vector3.Dot(Vector3.up, transform.up), 0);
            body.MoveRotation(currRot);
        }
    }
}
