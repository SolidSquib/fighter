using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Walk")]
public class SMovement_Walk : SPlayerMovementState
{
    public override void UpdateState(PlayerMovement playerMovement, Rigidbody playerRigidbody)
    {
        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);
        playerRigidbody.velocity = localMovementVector * baseMoveSpeed;
    }

    public override bool CheckShouldSwitchState(ref SPlayerMovementState newState) 
    { 
        return false; 
    }
}