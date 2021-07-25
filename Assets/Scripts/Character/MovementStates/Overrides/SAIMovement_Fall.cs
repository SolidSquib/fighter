using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Movement/Movement State/AI/Fall")]
public class SAIMovement_Fall : SCharacterMovementState
{
    public SAttribute moveSpeedAttribute = null;
    [SerializeField] private float _fallingGravityScale = 1.0f;
    [SerializeField] private float _jumpingGravityScale = 1.0f;
    public SCharacterMovementState _landedState;
    [Header("Transition Tuning")]
    [SerializeField] private float _groundCheckPreventionTime = 0.5f;

    private float _stateStartedTimestamp;

    public float fallGravityScale { get { return _fallingGravityScale; } }
    public float jumpGravityScale { get { return _jumpingGravityScale; } }

    public override bool CanJump() { return true; }
    public override bool IsFallingState() { return true; }

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
        _stateStartedTimestamp = Time.time;

        Animator animator = playerMovement.GetComponentInChildren<Animator>();
        animator.SetBool("isFalling", true);
    }

    public override void UpdateState(CharacterMovement playerMovement, Vector3 inputVelocity)
    {
        CharacterController controller = GetCharacterController(playerMovement);
        AbilitySystem abilitySystem = playerMovement.GetComponent<AbilitySystem>();
        float moveSpeedModifier = moveSpeedAttribute != null ? abilitySystem.GetAttributeCurrentValue(moveSpeedAttribute) : 1.0f;

        bool jumpFrame = inputVelocity.y > 0;

        Vector3 movementVector = new Vector3(playerMovement.inputVector.x * baseMoveSpeed * moveSpeedModifier, controller.velocity.y, playerMovement.inputVector.z * baseMoveSpeed * moveSpeedModifier);

        if (jumpFrame)
        {
            movementVector.y = inputVelocity.y;
        }

        // Apply gravity
        bool isFalling = !jumpFrame && controller.velocity.y <= 0;
        movementVector.y += (Physics.gravity.y * (isFalling ? fallGravityScale : jumpGravityScale) * Time.deltaTime);

        controller.Move(movementVector * Time.deltaTime);
    }

    public override bool CheckShouldSwitchState(CharacterMovement playerMovement, ref SCharacterMovementState newState)
    {
        if (Time.time < _stateStartedTimestamp + _groundCheckPreventionTime)
        {
            return false;
        }

        CharacterController controller = GetCharacterController(playerMovement);
        if (!controller.isGrounded)
        {
            return false;
        }

        newState = _landedState;
        return true;
    }
}