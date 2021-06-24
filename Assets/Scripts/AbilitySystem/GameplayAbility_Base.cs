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
    private bool _isActive;
    private GameplayEventData _eventData;
    private List<GameplayAbilityTask_Base> _activeTasks = new List<GameplayAbilityTask_Base>();
    public bool isActive { get { return _isActive; } private set { _isActive = value; } }
    public GameplayEventData eventData { get { return _eventData; } private set { _eventData = value; } }

    public virtual void ActivateAbility(GameplayEventData payload)
    {
        _eventData = payload;
        _isActive = true;
    }
    
    public virtual void EndAbility(bool wasCancelled)
    {
        foreach(var task in _activeTasks)
        {
            task.EndTask();
        }
        _eventData = new GameplayEventData();
        _isActive = false;
    }

    public virtual bool CanActivateAbility()
    {
        return true;
    }
}
