using System;
using System.Collections.Generic;
using UnityEngine;
using Fighter.Types;

[Serializable]
public struct GameplayAbilityTrigger
{
    public enum ETriggerMethod { GameplayEvent, TagPresent, TagAdded, TagRemoved }

    public Tag triggerTag;
    public ETriggerMethod triggerMethod;
}

public abstract class GameplayAbility_Base : ScriptableObject
{
    public delegate void AbilityDelegate(GameplayAbility_Base ability);
    public AbilityDelegate onAbilityEnded;

    public TagContainer abilityTags;
    public TagContainer activationOwnedTags;
    public TagContainer activationBlockedTags;
    public TagRequirementsContainer activationRequiredTags;
    public TagContainer cancelAbilitiesWithTags;
    public TagContainer blockAbilitiesWithTags;

    public bool retriggerInstancedAbility = false;

    [Header("Ability Triggers")]
    public GameplayAbilityTrigger[] tagTriggers;
    public bool activatedAbilityWhenGranted;

    public AbilitySystem abilitySystem { get; set; }

    // Internal 
    private ActiveAbilitySpec _spec;
    private GameplayEventData _eventData;
    private List<GameplayAbilityTask_Base> _activeTasks = new List<GameplayAbilityTask_Base>();
    public ActiveAbilitySpec spec { get { return _spec; } set { _spec = value; } }
    public bool isActive { get { return _spec.active; } }
    public GameplayEventData eventData { get { return _eventData; } private set { _eventData = value; } }

    public virtual void ActivateAbility(GameplayEventData payload)
    {
        _spec.active = true;
        _eventData = payload;
    }

    public virtual void EndAbility(bool wasCancelled)
    {
        foreach (var task in _activeTasks)
        {
            task.EndTask();
        }
        _eventData = new GameplayEventData();

        if (onAbilityEnded != null)
        {
            onAbilityEnded(this);
        }

        _spec.active = false;
    }

    public virtual bool CanActivateAbility()
    {
        return true;
    }

    // If this ability is bound to activate by input then this event will fire each time (except the initial activation via input) the bound action is activated
    public virtual void InputKeyDown() { }

    // If this ability is bound to activate by input then this event will fire each time the bound action is cancelled
    public virtual void InputKeyUp() { }
}
