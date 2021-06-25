using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Walk")]
public class SMovement_Walk : SPlayerMovementState
{
    public float _gravityScale = 1f;
    public SPlayerMovementState _fallingState;

    public override bool CanJump() { return true; }

    private CharacterController GetCharacterController(PlayerMovement playerMovement)
    {
        CharacterController controller = playerMovement.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"Movement state [{name}] is expecting a CharacterController but none is present.");
            return null;
        }

        return controller;
    }

    public override void UpdateState(PlayerMovement playerMovement, Vector3 inputVector)
    {
        CharacterController controller = GetCharacterController(playerMovement);

        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);
        Vector3 movementVector = new Vector3(localMovementVector.x * baseMoveSpeed, controller.velocity.y, localMovementVector.z * baseMoveSpeed);
        movementVector.y += inputVector.y;

        // Apply gravity
        movementVector.y += (Physics.gravity.y * _gravityScale * Time.deltaTime);

        controller.Move(movementVector * Time.deltaTime);
    }

    public override bool CheckShouldSwitchState(PlayerMovement playerMovement, ref SPlayerMovementState newState)
    {
        CharacterController controller = GetCharacterController(playerMovement);
        if (controller.isGrounded)
        {
            return false;
        }

        newState = _fallingState;
        return true;
    }
}