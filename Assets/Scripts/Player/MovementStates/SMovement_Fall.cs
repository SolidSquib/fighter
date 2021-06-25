using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Fall")]
public class SMovement_Fall : SPlayerMovementState
{
    [SerializeField] private float _simulatedFallMass = 5.0f;
    [SerializeField] private float _simulatedJumpMass = 0.5f;
    public SPlayerMovementState _landedState;
    [Header("Transition Tuning")]
    [SerializeField] private float _groundCheckPreventionTime = 0.5f;

    private float _stateStartedTimestamp;

    public float fallMass { get { return _simulatedFallMass; } }
    public float jumpMass { get { return _simulatedJumpMass; } }

    public override bool CanJump() { return true; }

    public override void EnterState()
    {
        _stateStartedTimestamp = Time.time;
    }

    public override void UpdateState(PlayerMovement playerMovement, Rigidbody playerRigidbody)
    {
        bool isFalling = playerRigidbody.velocity.y <= 0;

        float weight = isFalling ? fallMass : jumpMass;

        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);

        // Assign new velocity
        playerRigidbody.velocity = new Vector3(localMovementVector.x * baseMoveSpeed, playerRigidbody.velocity.y, localMovementVector.z * baseMoveSpeed);
        playerRigidbody.velocity += Vector3.up * Physics.gravity.y * weight * Time.deltaTime;
    }

    public override bool CheckShouldSwitchState(PlayerMovement playerMovement, Rigidbody playerRigidbody, ref SPlayerMovementState newState)
    {
        if (Time.time < _stateStartedTimestamp + _groundCheckPreventionTime)
        {
            return false;
        }

        if (!Fighter.GameplayStatics.TraceForGroundUnderneath(playerMovement, playerMovement.groundMask))
        {
            return false;
        }

        newState = _landedState;
        return true;
    }
}