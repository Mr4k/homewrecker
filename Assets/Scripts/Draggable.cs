using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    private bool _dragged;
    public Vector3 targetWorldPosition;
    public Quaternion targetWorldRotation;

    private void FixedUpdate()
    {
        if (_dragged)
        {
            var _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.MovePosition(targetWorldPosition);
            _rigidbody.MoveRotation(targetWorldRotation);
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
