using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Walk")]
public class SMovement_Walk : SPlayerMovementState
{
    public SPlayerMovementState _fallingState;

    public override bool CanJump() { return true; }

    public override void UpdateState(PlayerMovement playerMovement, Rigidbody playerRigidbody)
    {
        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);
        playerRigidbody.velocity = localMovementVector * baseMoveSpeed;
    }

    public override bool CheckShouldSwitchState(PlayerMovement playerMovement, Rigidbody playerRigidbody, ref SPlayerMovementState newState)
    {
        if (Fighter.GameplayStatics.TraceForGroundUnderneath(playerMovement, playerMovement.groundMask))
        {
            return false;
        }

        newState = _fallingState;
        return true;
    }
}