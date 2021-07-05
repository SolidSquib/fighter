using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AbilitySystem))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] GameplayDebuggerUI gameplayDebugger;
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

    void Start()
    {
        if (gameplayDebugger != null)
        {
            gameplayDebugger.BindAbilitySystemEvents(abilitySystem);
            gameplayDebugger.gameObject.SetActive(false);
        }
    }

    private void HandleInputAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "GameplayDebugger" && context.performed)
        {
            if (gameplayDebugger != null)
            {
                gameplayDebugger.gameObject.SetActive(!gameplayDebugger.gameObject.activeInHierarchy);
            }
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

        Transform attackOrigin;
        SpriteAttachments attachments = GetComponentInChildren<SpriteAttachments>();
        if (attachments != null && attachments.attachments.TryGetValue("AttackOrigin", out attackOrigin))
        {
            Vector3 amendedPosition = attachments.transform.position;
            
            DebugExtension.DebugWireSphere(attackOrigin.position, Color.red, 0.5f, 0, true);
        }
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
