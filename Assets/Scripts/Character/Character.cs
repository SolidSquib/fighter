using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AbilitySystem))]
[RequireComponent(typeof(CharacterMovement))]
public class Character : MonoBehaviour, IGameplayTagOwner
{
    [SerializeField] SCharacterData _characterData;

    public SCharacterData characterData { get { return _characterData; } private set { _characterData = value; } }
    public Animator animator { get; protected set; }
    public AbilitySystem abilitySystem { get; protected set; }
    public CharacterMovement characterMovement { get; protected set; }
    public CharacterController characterController { get; private set; }

    public TagContainer gameplayTags { get { return abilitySystem != null ? abilitySystem.gameplayTags : null; } }

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        abilitySystem = GetComponent<AbilitySystem>();
        abilitySystem.InitializeAbilitySystem(gameObject, gameObject);
        characterMovement = GetComponent<CharacterMovement>();
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    public bool CanJump()
    {
        return characterMovement.CanJump();
    }

    public bool HandleJump()
    {
        return characterMovement.Jump();
    }

    public bool IsJumpingOrFalling()
    {
        return characterMovement.IsJumpingOrFalling();
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
