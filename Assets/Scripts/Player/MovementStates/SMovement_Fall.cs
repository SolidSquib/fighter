using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Fall")]
public class SMovement_Fall : SPlayerMovementState
{
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _simulatedFallMass = 5.0f;
    [SerializeField] private float _simulatedJumpMass = 0.5f;

    public LayerMask groundMask { get { return _groundLayerMask; } }
    public float fallMass { get { return _simulatedFallMass; } }
    public float jumpMass { get { return _simulatedJumpMass; } }

    public override void UpdateState(PlayerMovement playerMovement, Rigidbody playerRigidbody)
    {
        bool isFalling = playerRigidbody.velocity.y <= 0;

        float weight = isFalling ? fallMass : jumpMass;

        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);

        // Assign new velocity
        playerRigidbody.velocity = new Vector3(localMovementVector.x * baseMoveSpeed, playerRigidbody.velocity.y, localMovementVector.z * baseMoveSpeed);
        playerRigidbody.velocity += Vector3.up * Physics.gravity.y * weight * Time.deltaTime;
    }

    public override bool CheckShouldSwitchState(ref SPlayerMovementState newState) 
    { 
        return false; 
    }
}