
using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EAbilityRemovalPolicy { CancelImmediately, WaitForEnd };
[Serializable]
public struct AbilitySpec
{
    public GameplayAbility_Base ability;
    public string inputActionBinding;
    public EAbilityRemovalPolicy removalPolicy;
};

public struct ActiveAbilitySpec
{
    public GameplayAbility_Base ability;
    public GameplayAbility_Base abilityTemplate;
    public EAbilityRemovalPolicy removalPolicy;
    public bool active;
    public bool inputActive;
}

public struct GameplayEventData
{
    AbilitySystem Source;
    AbilitySystem Target;
};

public class AbilitySystem : MonoBehaviour
{
    // Internal properties
    public Animator animator { get; private set; }
    public GameObject owner { get; private set; }
    public GameObject avatar { get; private set; }
    public GameObject abilitySystemObject { get; private set; }
    public bool initialized { get; private set; }
    TagContainer dynamicOwnedTags = new TagContainer();
    TagContainer activationBlockedTags = new TagContainer();

    List<ActiveAbilitySpec> ownedAbilities = new List<ActiveAbilitySpec>();
    Dictionary<string, List<ActiveAbilitySpec>> inputBoundAbilities = new Dictionary<string, List<ActiveAbilitySpec>>();

    public AbilitySpec[] startupAbilities;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput)
        {
            playerInput.onActionTriggered += ProcessInputAction;
        }
        else
        {
            Debug.LogWarning("No PlayerInput object found on the part of an AbilitySystem. Abilities will not be activated by input.");
        }

        foreach (var ability in startupAbilities)
        {
            if (ability.ability != null)
            {
                if (!GrantAbility(ability.ability, ability.inputActionBinding, ability.removalPolicy))
                {
                    Debug.LogWarning($"Failed to grant ability {ability.ability.ToString()}.");
                }
            }
        }
    }

    public void InitializeAbilitySystem(GameObject newOwner, GameObject newAvatar)
    {
        owner = newOwner;
        avatar = newAvatar;
        animator = newAvatar.GetComponentInChildren<Animator>();

        GameObject dummy = new GameObject("AbilitySystemGameObject");
        abilitySystemObject = Instantiate(dummy, owner.transform);

        initialized = true;
    }

    public bool HasAbility(GameplayAbility_Base ability)
    {
        foreach (var spec in ownedAbilities)
        {
            if (spec.abilityTemplate == ability)
            {
                return true;
            }
        }

        return false;
    }

    public bool GrantAbility(GameplayAbility_Base ability, string inputActionBinding = "", EAbilityRemovalPolicy removalPolicy = EAbilityRemovalPolicy.CancelImmediately)
    {
        AbilitySpec abilityToGrant;
        abilityToGrant.ability = ability;
        abilityToGrant.inputActionBinding = inputActionBinding;
        abilityToGrant.removalPolicy = removalPolicy;
        return GrantAbility(abilityToGrant);
    }

    public bool GrantAbility(AbilitySpec abilityToGrant)
    {
        if (HasAbility(abilityToGrant.ability))
        {
            Debug.LogWarning($"Attempted to add ability {abilityToGrant.ability.name} when it has already been previously granted.");
            return false;
        }

        ActiveAbilitySpec newSpec = new ActiveAbilitySpec();
        newSpec.ability = Instantiate(abilityToGrant.ability);
        newSpec.ability.name = $"{abilityToGrant.ability.name}_instance";
        newSpec.ability.abilitySystem = this;
        newSpec.abilityTemplate = abilityToGrant.ability;
        newSpec.removalPolicy = abilityToGrant.removalPolicy;
        newSpec.active = false;
        newSpec.inputActive = false;

        ownedAbilities.Add(newSpec);
        Debug.Log(abilityToGrant.ability.name);

        if (abilityToGrant.inputActionBinding.Length > 0)
        {
            if (!inputBoundAbilities.ContainsKey(abilityToGrant.inputActionBinding))
            {
                inputBoundAbilities.Add(abilityToGrant.inputActionBinding, new List<ActiveAbilitySpec>());
            }

            List<ActiveAbilitySpec> currentBoundAbilities = inputBoundAbilities[abilityToGrant.inputActionBinding];
            if (!currentBoundAbilities.Contains(newSpec))
            {
                currentBoundAbilities.Add(newSpec);
            }
        }

        return true;
    }

    public int ProcessGameplayEvent(Tag eventTag, GameplayEventData payload)
    {
        int numActivatedAbilities = 0;



        return numActivatedAbilities;
    }
    
    private void ProcessInputAction(InputAction.CallbackContext context)
    {
        if (!inputBoundAbilities.ContainsKey(context.action.name))
        {
            return;
        }

        List<ActiveAbilitySpec> abilitiesToActivate = inputBoundAbilities[context.action.name];

        foreach (var spec in abilitiesToActivate)
        {
            TryActivateAbilitySpec(spec, new GameplayEventData());
        }
    }

    private bool TryActivateAbilitySpec(ActiveAbilitySpec spec, GameplayEventData payload)
    {
        if (!CanActivateAbilitySpec(spec))
        {
            return false;
        }

        spec.ability.ActivateAbility(payload);
        return true;
    }

    private bool CanActivateAbilitySpec(ActiveAbilitySpec spec)
    {
        if (spec.ability == null)
        {
            Debug.LogWarning($"Unable to start a null ability.");
            return false;
        }

        if (!dynamicOwnedTags.AllTagsMatch(spec.ability.activationRequiredTags.required) || dynamicOwnedTags.AnyTagsMatch(spec.ability.activationRequiredTags.ignored))
        {
            Debug.LogWarning($"Unable to activate ability \"{spec.ability.name}\", tag requirements not met.");
            return false;
        }

        if (activationBlockedTags.AnyTagsMatch(spec.ability.abilityTags))
        {
            Debug.LogWarning($"Unable to activate ability \"{spec.ability.name}\", ability blocked by tags.");
            return false;
        }

        if (!spec.ability.CanActivateAbility())
        {
            Debug.LogWarning($"Unable to activate ability \"{spec.ability.name}\", because its implementation of CanActivateAbility returned false.");
            return false;
        }

        return true;
    }
}
