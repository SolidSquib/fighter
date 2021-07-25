using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : Character
{
    [SerializeField] GameplayDebuggerUI gameplayDebugger;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        GetComponent<PlayerInput>().onActionTriggered += HandleInputAction;
    }

    void Start()
    {
        if (gameplayDebugger != null)
        {
            gameplayDebugger.BindAbilitySystemEvents(abilitySystem);
            gameplayDebugger.gameObject.SetActive(true);
            gameplayDebugger.GetComponent<Animator>().SetBool("isOpen", false);
        }
    }

    private void HandleInputAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "GameplayDebugger" && context.performed)
        {
            if (gameplayDebugger != null)
            {
                Animator animator = gameplayDebugger.GetComponent<Animator>();
                animator.SetBool("isOpen", !animator.GetBool("isOpen"));
            }
        }
        else if (context.action.name == "Move")
        {
            Vector2 stickValue = context.ReadValue<Vector2>();
            characterMovement.inputVector = new Vector3(stickValue.x, 0, stickValue.y);
        }
    }
}
