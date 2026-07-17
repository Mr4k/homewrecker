using UnityEngine;
using KinematicCharacterController;

public struct FirstPersonInputs
{
    public Vector2 Move;
    public float LookYaw;
    public bool Jump;
}

public class FirstPersonCharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;
    public float MoveSpeed = 8f;
    public float MovementSharpness = 15f;
    public float AirAcceleration = 25f;
    public float JumpSpeed = 10f;
    public Vector3 Gravity = new Vector3(0f, -30f, 0f);

    private Vector3 _moveInput;
    private float _yaw;
    private bool _jumpRequested;

    private void Awake()
    {
        _yaw = transform.eulerAngles.y;
        Motor.CharacterController = this;
    }

    public void SetInputs(ref FirstPersonInputs inputs)
    {
        _yaw += inputs.LookYaw;
        _moveInput = Quaternion.Euler(0f, _yaw, 0f) * Vector3.ClampMagnitude(new Vector3(inputs.Move.x, 0f, inputs.Move.y), 1f);
        _jumpRequested |= inputs.Jump;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 targetVelocity = Motor.GetDirectionTangentToSurface(_moveInput, Motor.GroundingStatus.GroundNormal) * (_moveInput.magnitude * MoveSpeed);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-MovementSharpness * deltaTime));

            if (_jumpRequested)
            {
                Motor.ForceUnground();
                currentVelocity += Motor.CharacterUp * JumpSpeed - Vector3.Project(currentVelocity, Motor.CharacterUp);
            }
        }
        else
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);
            Vector3 addedVelocity = _moveInput * (AirAcceleration * deltaTime);
            if (planarVelocity.magnitude < MoveSpeed)
                addedVelocity = Vector3.ClampMagnitude(planarVelocity + addedVelocity, MoveSpeed) - planarVelocity;
            else if (Vector3.Dot(planarVelocity, addedVelocity) > 0f)
                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, planarVelocity.normalized);

            currentVelocity += addedVelocity + Gravity * deltaTime;
        }
        _jumpRequested = false;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public void AfterCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public bool IsColliderValidForCollisions(Collider coll) { return true; }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
}
