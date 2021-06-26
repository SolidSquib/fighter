using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Movement State/Defaults/Fall")]
public class SMovement_Fall : SPlayerMovementState
{
    [SerializeField] private float _fallingGravityScale = 1.0f;
    [SerializeField] private float _jumpingGravityScale = 1.0f;
    public SPlayerMovementState _landedState;
    [Header("Transition Tuning")]
    [SerializeField] private float _groundCheckPreventionTime = 0.5f;

    private float _stateStartedTimestamp;

    public float fallGravityScale { get { return _fallingGravityScale; } }
    public float jumpGravityScale { get { return _jumpingGravityScale; } }

    public override bool CanJump() { return true; }
    public override bool IsFallingState() { return true; }

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

    public override void EnterState(PlayerMovement playerMovement)
    {
        _stateStartedTimestamp = Time.time;

        Animator animator = playerMovement.GetComponentInChildren<Animator>();
        animator.SetBool("isFalling", true);
    }

    public override void UpdateState(PlayerMovement playerMovement, Vector3 inputVector)
    {
        CharacterController controller = GetCharacterController(playerMovement);

        bool jumpFrame = inputVector.y > 0;

        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, playerMovement.transform, playerMovement.inputVector);
        Vector3 movementVector = new Vector3(localMovementVector.x * baseMoveSpeed, controller.velocity.y, localMovementVector.z * baseMoveSpeed);

        if (jumpFrame)
        {
            movementVector.y = inputVector.y;
        }

        // Apply gravity
        bool isFalling = !jumpFrame && controller.velocity.y <= 0;
        movementVector.y += (Physics.gravity.y * (isFalling ? fallGravityScale : jumpGravityScale) * Time.deltaTime);

        controller.Move(movementVector * Time.deltaTime);
    }

    public override bool CheckShouldSwitchState(PlayerMovement playerMovement, ref SPlayerMovementState newState)
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