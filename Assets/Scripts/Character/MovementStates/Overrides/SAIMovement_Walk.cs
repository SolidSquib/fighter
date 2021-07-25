using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Movement/Movement State/AI/Walk")]
public class SAIMovement_Walk : SCharacterMovementState
{
    public SAttribute moveSpeedAttribute = null;
    public float _gravityScale = 1f;
    public SCharacterMovementState _fallingState;

    public override bool CanJump() { return true; }

    private CharacterController GetCharacterController(CharacterMovement playerMovement)
    {
        CharacterController controller = playerMovement.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"Movement state [{name}] is expecting a CharacterController but none is present.");
            return null;
        }

        return controller;
    }

    public override void EnterState(CharacterMovement playerMovement)
    {
        Animator animator = playerMovement.GetComponentInChildren<Animator>();
        animator.SetBool("isFalling", false);
    }

    public override void UpdateState(CharacterMovement playerMovement, Vector3 inputVelocity)
    {
        CharacterController controller = GetCharacterController(playerMovement);
        AbilitySystem abilitySystem = playerMovement.GetComponent<AbilitySystem>();
        float moveSpeedModifier = moveSpeedAttribute != null ? abilitySystem.GetAttributeCurrentValue(moveSpeedAttribute) : 1.0f;

        Vector3 movementVector = new Vector3(playerMovement.inputVector.x * baseMoveSpeed * moveSpeedModifier, controller.velocity.y, playerMovement.inputVector.z * baseMoveSpeed * moveSpeedModifier);
        movementVector.y += inputVelocity.y;

        // Apply gravity
        movementVector.y += (Physics.gravity.y * _gravityScale * Time.deltaTime);

        controller.Move(movementVector * Time.deltaTime);
    }

    public override bool CheckShouldSwitchState(CharacterMovement playerMovement, ref SCharacterMovementState newState)
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