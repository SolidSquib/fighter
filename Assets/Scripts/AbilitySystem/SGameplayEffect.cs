using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SModifierMagnitudeCalculation : ScriptableObject
{
    public abstract float GetModifierMagnitude();
}

public enum EModifierMethod { Add, Multiply, Divide, Override }
public enum EModifierCalculation { ScalableFloat, AttributeBased, CustomCalculationClass, SetByCaller }

[System.Serializable]
public struct EffectModifierMagnitudeInfo
{
    public EModifierMethod method;
    public EModifierCalculation magnitudeCalculation;
    public Tag setByCallerTag;
    public float baseMagnitude;
    public SModifierMagnitudeCalculation customCalculationClass;
}

public struct CachedEffectModifierMagnitude
{
    public SAttribute attribute;
    public EModifierMethod method;
    public float magnitude;
}

public class GameplayEffectSpec
{
    public AbilitySystem source { get; private set; } = null;
    public AbilitySystem target { get; set; } = null;
    public Dictionary<Tag, float> setByCallerValues { get; private set; } = new Dictionary<Tag, float>();
    public SGameplayEffect effectTemplate { get; private set; } = null;
    public float applicationTime { get; set; } = 0;

    public List<CachedEffectModifierMagnitude> cachedModifiers = new List<CachedEffectModifierMagnitude>();

    public GameplayEffectSpec(AbilitySystem source, SGameplayEffect effect)
    {
        this.source = source;
        this.effectTemplate = effect;
    }

    public GameplayEffectSpec(AbilitySystem source, AbilitySystem target, SGameplayEffect effect)
    {
        this.source = source;
        this.target = target;
        this.effectTemplate = effect;
    }
}

[System.Serializable]
public struct GameplayEffectAttributeModifier
{
    public SAttribute attribute;
    public EffectModifierMagnitudeInfo magnitude;
}

[CreateAssetMenu(menuName = "Ability System/Gameplay Effect")]
public class SGameplayEffect : ScriptableObject
{
    public enum EEffectDurationPolicy { Instant, Infinite, Duration }

    public EEffectDurationPolicy durationPolicy;
    public EffectModifierMagnitudeInfo duration;
    public TagContainer effectTags = new TagContainer();
    public TagContainer grantedTags = new TagContainer();

    [Header("Application Requirements")]
    public TagRequirementsContainer ongoingTagRequirements;
    public TagRequirementsContainer applicationTagRequirements;
    public TagRequirementsContainer removalTagRequirements;
    public TagContainer removeGameplayEffectsWithTags;

    [Header("Attribute Modifiers")]
    public List<GameplayEffectAttributeModifier> modifiers;

    [Header("Granted Abilities")]
    public List<GrantedAbilityInfo> grantedAbilities;
}
