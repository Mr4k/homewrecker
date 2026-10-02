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

    public void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            positionControllers[i] = new PIDController(positionControllerParams);
        }
    }

    private void FixedUpdate()
    {
        if (_dragged)
        {
            var _rigidbody = GetComponent<Rigidbody>();
            // todo maybe we want to normalize this can be boxy
            _rigidbody.AddForce(
                positionControllers[0].Update(Time.fixedDeltaTime, _rigidbody.transform.position.x, targetWorldPosition.x),
                0,//positionControllers[1].Update(Time.fixedDeltaTime, _rigidbody.transform.position.y, targetWorldPosition.y),
                positionControllers[2].Update(Time.fixedDeltaTime, _rigidbody.transform.position.z, targetWorldPosition.z)
            , ForceMode.Acceleration);
        }
    }

    public virtual void Pickup()
    {
        _dragged = true;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        for (int i = 0; i < 3; i++)
        {
            positionControllers[i].Reset();
        }
    }

    public virtual void Drop()
    {
        _dragged = false;
        var _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
    }
}
