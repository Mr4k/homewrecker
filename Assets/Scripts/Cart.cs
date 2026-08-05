using System.Collections.Generic;
using UnityEngine;

public class Cart : Draggable
{
    private Wheel[] _wheels;

    public void Start()
    {
        _wheels = GetComponentsInChildren<Wheel>();
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
        }
        base.EndDrag();
    }
}
