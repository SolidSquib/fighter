using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AbilitySystem))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCharacter : MonoBehaviour
{
    public Animator animator { get; protected set; }
    public AbilitySystem abilitySystem { get; protected set; }
    public PlayerMovement playerMovement { get; protected set; }
    public CharacterController characterController { get; private set; }

    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        abilitySystem = GetComponent<AbilitySystem>();
        abilitySystem.InitializeAbilitySystem(gameObject, gameObject);
        playerMovement = GetComponent<PlayerMovement>();
        characterController = GetComponent<CharacterController>();

        GetComponent<PlayerInput>().onActionTriggered += HandleInputAction;
    }

    private void HandleInputAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "Dash")
        {
            /* Vector2 stickValue = context.ReadValue<Vector2>();
            AbilityStatics.SendGameplayEventToAbilitySystem(abilitySystem, ) */
        }
        else if (context.action.name == "Move")
        {
            Vector2 stickValue = context.ReadValue<Vector2>();
            playerMovement.inputVector = new Vector3(stickValue.x, 0, stickValue.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    public bool CanJump()
    {
        return playerMovement.CanJump();
    }

    public bool HandleJump()
    {
        return playerMovement.Jump();
    }

    public bool IsJumpingOrFalling()
    {
        return playerMovement.IsJumpingOrFalling();
    }

    void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        Vector3 horizontalMovement = characterController.velocity;
        horizontalMovement.y = 0;

        animator.SetFloat("moveSpeed", horizontalMovement.magnitude);
        animator.SetFloat("verticalSpeed", characterController.velocity.y);
    }
}
