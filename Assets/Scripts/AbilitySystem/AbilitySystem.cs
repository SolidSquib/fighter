
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

public class ActiveAbilitySpec
{
    public GameplayAbility_Base ability { get; set; } = null;
    public GameplayAbility_Base abilityTemplate { get; set; } = null;
    public EAbilityRemovalPolicy removalPolicy { get; set; } = EAbilityRemovalPolicy.CancelImmediately;
    public bool active { get; set; } = false;
    public bool inputActive { get; set; } = false;
}

public struct GameplayEventData
{
    AbilitySystem Source;
    AbilitySystem Target;
};

public class AbilitySystem : MonoBehaviour
{
    public delegate void AbilityDelegate(GameplayAbility_Base ability);
    public delegate void GameplayEventNotify(Tag eventTag, GameplayEventData eventData);
    public AbilityDelegate onAbilityEnded;
    public AbilityDelegate onAbilityActivated;
    public AbilityDelegate onAbilityActivationFailed;
    private Dictionary<Tag, GameplayEventNotify> genericGameplayEventCallbacks;

    // Internal properties
    public Animator animator { get; private set; }
    public GameObject owner { get; private set; }
    public GameObject avatar { get; private set; }
    public GameObject abilitySystemObject { get; private set; }
    public bool initialized { get; private set; }
    TagContainer dynamicOwnedTags = new TagContainer();
    TagContainer activationBlockedTags = new TagContainer();

    List<ActiveAbilitySpec> _ownedAbilities = new List<ActiveAbilitySpec>();
    Dictionary<string, List<ActiveAbilitySpec>> _inputBoundAbilities = new Dictionary<string, List<ActiveAbilitySpec>>();

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

    public ActiveAbilitySpec GetAbilitySpecFromAbility(GameplayAbility_Base ability)
    {
        return _ownedAbilities.Find(item => item.ability == ability);
    }

    public bool HasAbility(GameplayAbility_Base ability)
    {
        return _ownedAbilities.Find(item => item.abilityTemplate == ability || item.ability == ability) != null;
    }

    public List<ActiveAbilitySpec> GetActiveAbilitySpecs()
    {
        return _ownedAbilities.FindAll(item => item.active);
    }

    public List<ActiveAbilitySpec> GetAbilitySpecsBoundToInput(string inputAction)
    {
        return _inputBoundAbilities.ContainsKey(inputAction) ? _inputBoundAbilities[inputAction] : new List<ActiveAbilitySpec>();
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
        newSpec.ability.spec = newSpec;
        newSpec.abilityTemplate = abilityToGrant.ability;
        newSpec.removalPolicy = abilityToGrant.removalPolicy;

        _ownedAbilities.Add(newSpec);

        if (abilityToGrant.inputActionBinding.Length > 0)
        {
            if (!_inputBoundAbilities.ContainsKey(abilityToGrant.inputActionBinding))
            {
                _inputBoundAbilities.Add(abilityToGrant.inputActionBinding, new List<ActiveAbilitySpec>());
            }

            List<ActiveAbilitySpec> currentBoundAbilities = _inputBoundAbilities[abilityToGrant.inputActionBinding];
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
        List<ActiveAbilitySpec> eventResponders = _ownedAbilities.FindAll(spec =>
        {
            return spec.ability.tagTriggers.Find(trigger => eventTag.IsChildOf(trigger.triggerTag) && trigger.triggerMethod == GameplayAbilityTrigger.ETriggerMethod.GameplayEvent) != null;
        });

        foreach (var spec in eventResponders)
        {
            TryActivateAbilitySpec(spec, payload);
        }

        GameplayEventNotify genericCallbacks;
        if (genericGameplayEventCallbacks.TryGetValue(eventTag, out genericCallbacks))
        {
            genericCallbacks(eventTag, payload);
        }

        return numActivatedAbilities;
    }

    public void RegisterGenericGameplayEventCallback(Tag eventTag, GameplayEventNotify callback)
    {
        GameplayEventNotify existingCallback;
        if (genericGameplayEventCallbacks.TryGetValue(eventTag, out existingCallback))
        {
            existingCallback += callback;
        }
        else
        {
            genericGameplayEventCallbacks.Add(eventTag, callback);
        }
    }

    public void UnregisterGenericGameplayEventCallback(Tag eventTag, GameplayEventNotify callback)
    {
        GameplayEventNotify existingCallback;
        if (genericGameplayEventCallbacks.TryGetValue(eventTag, out existingCallback))
        {
            existingCallback -= callback;

            if (existingCallback == null)
            {
                genericGameplayEventCallbacks.Remove(eventTag);
            }
        }
    }

    private void ProcessInputAction(InputAction.CallbackContext context)
    {
        List<ActiveAbilitySpec> abilitiesToActivate = GetAbilitySpecsBoundToInput(context.action.name);

        foreach (var spec in abilitiesToActivate)
        {
            if (context.performed)
            {
                if (!TryActivateAbilitySpec(spec, new GameplayEventData()) && spec.active && !spec.inputActive)
                {
                    spec.ability.InputKeyDown();
                }

                spec.inputActive = true;
            }
            else if (context.canceled)
            {
                if (spec.active && spec.inputActive)
                {
                    spec.ability.InputKeyUp();
                }

                spec.inputActive = false;
            }
        }
    }

    private bool TryActivateAbilitySpec(ActiveAbilitySpec spec, GameplayEventData payload)
    {
        if (!CanActivateAbilitySpec(spec))
        {
            return false;
        }

        ActivateAbilitySpec_Internal(spec, payload);
        return true;
    }

    private void ActivateAbilitySpec_Internal(ActiveAbilitySpec spec, GameplayEventData payload)
    {
        spec.ability.onAbilityEnded = NotifyAbilityEnded;
        spec.ability.ActivateAbility(payload);
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

        if (spec.active && !spec.ability.retriggerInstancedAbility)
        {
            return false;
        }

        return true;
    }

    private void NotifyAbilityEnded(GameplayAbility_Base ability)
    {
        if (HasAbility(ability))
        {
            ability.onAbilityEnded = null;

            if (onAbilityEnded != null)
            {
                onAbilityEnded(ability);
            }
        }
    }
}
