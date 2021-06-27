
using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EAbilityRemovalPolicy { CancelImmediately, WaitForEnd };
[Serializable]
public struct GrantedAbilityInfo
{
    public SGameplayAbility ability;
    public string inputActionBinding;
    public EAbilityRemovalPolicy removalPolicy;
};

public class ActiveAbilitySpec
{
    public SGameplayAbility ability { get; set; } = null;
    public SGameplayAbility abilityTemplate { get; set; } = null;
    public EAbilityRemovalPolicy removalPolicy { get; set; } = EAbilityRemovalPolicy.CancelImmediately;
    public bool active { get; set; } = false;
    public bool inputActive { get; set; } = false;
}

public struct GameplayEventData
{
    AbilitySystem Source;
    AbilitySystem Target;
};

public struct ActiveEffectHandle
{
    public int id;
    public AbilitySystem target;
    public AbilitySystem source;

    public bool IsValid()
    {
        return id > -1 && target != null && source != null && target.GetActiveGameplayEffectSpecFromHandle(this) != null;
    }

    public static bool operator ==(ActiveEffectHandle a, ActiveEffectHandle b)
    {
        return a.id == b.id && a.source == b.source && a.target == b.target;
    }

    public static bool operator !=(ActiveEffectHandle a, ActiveEffectHandle b)
    {
        return a.id != b.id || a.source != b.source || a.target != b.target;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ActiveEffectHandle handle))
        {
            return false;
        }

        return this == handle;
    }

    public override int GetHashCode()
    {
        unchecked // doesn't check for overflow, just wraps
        {
            int hash = 17;
            hash *= (23 + id.GetHashCode());
            hash *= (23 + target.GetHashCode());
            hash *= (23 + source.GetHashCode());
            return hash;
        }
    }
    public static ActiveEffectHandle Invalid = new ActiveEffectHandle { id = -1, source = null, target = null };
}

public class AbilitySystem : MonoBehaviour
{
    public delegate void AbilityDelegate(SGameplayAbility ability);
    public delegate void GameplayEventNotify(Tag eventTag, GameplayEventData eventData);
    public AbilityDelegate onAbilityEnded;
    public AbilityDelegate onAbilityActivated;
    public AbilityDelegate onAbilityActivationFailed;
    private Dictionary<Tag, GameplayEventNotify> _genericGameplayEventCallbacks;
    private SAttributeSet _attributeSetInstance;
    private ActiveGameplayEffectsContainer _activeGameplayEffects = null;

    // Internal properties
    public Animator animator { get; private set; }
    public GameObject owner { get; private set; }
    public GameObject avatar { get; private set; }
    public GameObject abilitySystemObject { get; private set; }
    public bool initialized { get; private set; }
    CountingTagContainer _dynamicOwnedTags = new CountingTagContainer();
    CountingTagContainer _activationBlockedTags = new CountingTagContainer();

    public CountingTagContainer dynamicOwnedTags { get { return _dynamicOwnedTags; } private set { _dynamicOwnedTags = value; } }
    public CountingTagContainer activationBlockedTags { get { return _activationBlockedTags; } private set { _activationBlockedTags = value; } }

    List<ActiveAbilitySpec> _ownedAbilities = new List<ActiveAbilitySpec>();
    Dictionary<string, List<ActiveAbilitySpec>> _inputBoundAbilities = new Dictionary<string, List<ActiveAbilitySpec>>();

    [SerializeField] private GrantedAbilityInfo[] _startupAbilities;
    [SerializeField] private SGameplayEffect[] _startupEffects;
    [SerializeField] private SAttributeSet _attributeSet;

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

        foreach (var ability in _startupAbilities)
        {
            if (ability.ability != null)
            {
                if (!GrantAbility(ability.ability, ability.inputActionBinding, ability.removalPolicy))
                {
                    Debug.LogWarning($"Failed to grant ability {ability.ability.ToString()}.");
                }
            }
        }

        foreach (var effect in _startupEffects)
        {
            if (effect != null)
            {
                ApplyGameplayEffectToSelf(effect);
            }
        }

        if (_attributeSet != null)
        {
            _attributeSetInstance = ScriptableObject.Instantiate(_attributeSet);
        }

        _activeGameplayEffects = new ActiveGameplayEffectsContainer(this);
    }

    private void Update()
    {
        _activeGameplayEffects.RemoveExpiredGameplayEffects();
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

    public ActiveAbilitySpec GetAbilitySpecFromAbility(SGameplayAbility ability)
    {
        return _ownedAbilities.Find(item => item.ability == ability);
    }

    public bool HasAbility(SGameplayAbility ability)
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

    public bool GrantAbility(SGameplayAbility ability, string inputActionBinding = "", EAbilityRemovalPolicy removalPolicy = EAbilityRemovalPolicy.CancelImmediately)
    {
        GrantedAbilityInfo abilityToGrant;
        abilityToGrant.ability = ability;
        abilityToGrant.inputActionBinding = inputActionBinding;
        abilityToGrant.removalPolicy = removalPolicy;
        return GrantAbility(abilityToGrant);
    }

    public bool GrantAbility(GrantedAbilityInfo abilityToGrant)
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
        if (_genericGameplayEventCallbacks.TryGetValue(eventTag, out genericCallbacks))
        {
            genericCallbacks(eventTag, payload);
        }

        return numActivatedAbilities;
    }

    public void RegisterGenericGameplayEventCallback(Tag eventTag, GameplayEventNotify callback)
    {
        GameplayEventNotify existingCallback;
        if (_genericGameplayEventCallbacks.TryGetValue(eventTag, out existingCallback))
        {
            existingCallback += callback;
        }
        else
        {
            _genericGameplayEventCallbacks.Add(eventTag, callback);
        }
    }

    public void UnregisterGenericGameplayEventCallback(Tag eventTag, GameplayEventNotify callback)
    {
        GameplayEventNotify existingCallback;
        if (_genericGameplayEventCallbacks.TryGetValue(eventTag, out existingCallback))
        {
            existingCallback -= callback;

            if (existingCallback == null)
            {
                _genericGameplayEventCallbacks.Remove(eventTag);
            }
        }
    }

    public void RegisterOnAttributeChangedCallback(SAttribute attribute, SAttributeSet.AttributeDelegate callback)
    {
        if (_attributeSetInstance != null)
        {
            _attributeSetInstance.RegisterOnAttributeChangedCallback(attribute, callback);
        }
    }

    public void UnregisterOnAttributeChangedCallback(SAttribute attribute, SAttributeSet.AttributeDelegate callback)
    {
        if (_attributeSetInstance != null)
        {
            _attributeSetInstance.UnregisterOnAttributeChangedCallback(attribute, callback);
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

        if (!_dynamicOwnedTags.AllTagsMatch(spec.ability.activationRequiredTags.required) || _dynamicOwnedTags.AnyTagsMatch(spec.ability.activationRequiredTags.ignored))
        {
            Debug.LogWarning($"Unable to activate ability \"{spec.ability.name}\", tag requirements not met.");
            return false;
        }

        if (_activationBlockedTags.AnyTagsMatch(spec.ability.abilityTags))
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

    private void NotifyAbilityEnded(SGameplayAbility ability)
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

    public GameplayEffectSpec MakeGameplayEffectSpec(SGameplayEffect effect)
    {
        return new GameplayEffectSpec(this, effect);
    }

    public GameplayEffectSpec GetActiveGameplayEffectSpecFromHandle(ActiveEffectHandle handle)
    {
        GameplayEffectSpec spec;
        if (_activeGameplayEffects.TryGetSpecFromHandle(handle, out spec))
        {
            return spec;
        }

        return null;
    }

    public ActiveEffectHandle ApplyGameplayEffectToSelf(SGameplayEffect effect)
    {
        if (effect != null)
        {
            GameplayEffectSpec spec = MakeGameplayEffectSpec(effect);
            return ApplyGameplayEffectSpecToSelf(spec);
        }

        return ActiveEffectHandle.Invalid;
    }

    public ActiveEffectHandle ApplyGameplayEffectToTarget(SGameplayEffect effect, AbilitySystem target)
    {
        if (effect != null)
        {
            GameplayEffectSpec spec = MakeGameplayEffectSpec(effect);
            return ApplyGameplayEffectSpecToTarget(spec, target);
        }

        return ActiveEffectHandle.Invalid;
    }

    public ActiveEffectHandle ApplyGameplayEffectSpecToSelf(GameplayEffectSpec spec)
    {
        if (spec == null)
        {
            return ActiveEffectHandle.Invalid;
        }

        spec.target = this;

        /* Check the attribute set requirements and make sure all attributes are valid before 
         * commiting to application. */
        foreach (GameplayEffectAttributeModifier modifier in spec.effectTemplate.modifiers)
        {
            if (modifier.attribute == null)
            {
                Debug.LogWarning($"{spec.effectTemplate.name} has a null modifier or modifier attribute.");
                return ActiveEffectHandle.Invalid;
            }
        }

        // TODO: maybe add a "chance to add" property to effects if required.

        if (!spec.effectTemplate.applicationTagRequirements.RequirementsMet(_dynamicOwnedTags))
        {
            return ActiveEffectHandle.Invalid;
        }

        if (!spec.effectTemplate.removalTagRequirements.IsEmpty() && spec.effectTemplate.removalTagRequirements.RequirementsMet(_dynamicOwnedTags))
        {
            return ActiveEffectHandle.Invalid;
        }

        RecalculateModifierMagitudesForSpec(spec);

        if (spec.effectTemplate.durationPolicy == SGameplayEffect.EEffectDurationPolicy.Instant)
        {
            foreach (CachedEffectModifierMagnitude mod in spec.cachedModifiers)
            {
                _attributeSetInstance.ExecuteAttributeModifier(mod);
            }

            _activeGameplayEffects.RemoveEffectsWithTags(spec.effectTemplate.removeGameplayEffectsWithTags);
        }
        else
        {
            // TODO handle infinite and duration based effects.
            ActiveEffectHandle handle = _activeGameplayEffects.AddActiveGameplayEffect(spec);
            
            return handle;
        }

        return ActiveEffectHandle.Invalid;
    }

    public ActiveEffectHandle ApplyGameplayEffectSpecToTarget(GameplayEffectSpec spec, AbilitySystem target)
    {
        if (target != null)
        {
            return target.ApplyGameplayEffectSpecToSelf(spec);
        }

        return ActiveEffectHandle.Invalid;
    }

    protected void RecalculateModifierMagitudesForSpec(GameplayEffectSpec spec)
    {
        if (spec.effectTemplate == null)
        {
            Debug.LogWarning("Unable to calculate magnitudes of a null Gameplay Effect");
            return;
        }

        spec.cachedModifiers.Clear();

        foreach (GameplayEffectAttributeModifier modifierInfo in spec.effectTemplate.modifiers)
        {
            CachedEffectModifierMagnitude calculatedModifier = new CachedEffectModifierMagnitude() { attribute = modifierInfo.attribute, method = modifierInfo.magnitude.method };

            switch (modifierInfo.magnitude.magnitudeCalculation)
            {
                case EModifierCalculation.ScalableFloat:
                    calculatedModifier.magnitude = modifierInfo.magnitude.baseMagnitude;
                    break;
                case EModifierCalculation.AttributeBased:
                    calculatedModifier.magnitude = _attributeSetInstance.GetAttributeCurrentValue(modifierInfo.attribute);
                    break;
                case EModifierCalculation.CustomCalculationClass:
                    if (modifierInfo.magnitude.customCalculationClass != null)
                    {
                        calculatedModifier.magnitude = modifierInfo.magnitude.customCalculationClass.GetModifierMagnitude();
                    }
                    break;
                case EModifierCalculation.SetByCaller:
                    if (spec.setByCallerValues.ContainsKey(modifierInfo.magnitude.setByCallerTag))
                    {
                        calculatedModifier.magnitude = spec.setByCallerValues[modifierInfo.magnitude.setByCallerTag];
                    }
                    break;
            }

            spec.cachedModifiers.Add(calculatedModifier);
        }
    }
}
