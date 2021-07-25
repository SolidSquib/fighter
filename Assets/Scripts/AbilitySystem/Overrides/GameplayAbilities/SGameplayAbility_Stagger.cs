using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = GameplayAbilitySystem.Strings.AbilitySystemCreateMenuPrefix + GameplayAbilitySystem.Strings.GameplayAbilityCreateMenuPrefix + "Stagger")]
public class SGameplayAbility_Stagger : SGameplayAbility
{
    public SGameplayEffect staggerEffect;
    public Tag stunDurationTag;
    public float stunDurationMagnitude;

    private Coroutine animationCoroutine;
    private Coroutine effectCoroutine;

    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);

        ResetAbilityInstance();

        ClipTransition staggerAnimation = abilitySystem.GetComponent<Character>().characterData.staggerAnimation;

        if (staggerAnimation == null)
        {
            EndAbility(true);
            return;
        }

        if (staggerEffect == null)
        {
            EndAbility(true);
            return;
        }

        GameplayEffectSpec effectSpec = abilitySystem.MakeGameplayEffectSpec(staggerEffect);
        effectSpec.setByCallerValues.Add(stunDurationTag, stunDurationMagnitude);

        effectCoroutine = abilitySystem.StartCoroutine(GameplayAbilitySystem.WaitEffectRemoved.Start(abilitySystem.ApplyGameplayEffectSpecToSelf(effectSpec), OnEffectRemoved));
        animationCoroutine = abilitySystem.StartCoroutine(PlayStagger(staggerAnimation));
    }

    public override void EndAbility(bool wasCancelled)
    {
        base.EndAbility(wasCancelled);
        
        ResetAbilityInstance();

        HybridAnimancerComponent animancerComponent = abilitySystem.GetComponentInChildren<HybridAnimancerComponent>();
        animancerComponent.Play(animancerComponent.Controller, 0);
    }

    IEnumerator PlayStagger(ClipTransition transition)
    {
        if (transition != null)
        {
            ClipState.Transition animation = transition.Transition;
            yield return abilitySystem.GetComponentInChildren<HybridAnimancerComponent>().Play(animation);
        }

        ClipTransition stunnedAnimation = abilitySystem.GetComponent<Character>().characterData.stunnedAnimation;
        if (stunnedAnimation != null)
        {
            PlayStun(stunnedAnimation);
        }
    }

    void PlayStun(ClipTransition stunAnimation)
    {
        ClipState.Transition animation = stunAnimation.Transition;
        abilitySystem.GetComponentInChildren<HybridAnimancerComponent>().Play(animation);
    }

    void OnEffectRemoved()
    {
        EndAbility(false);
    }

    void ResetAbilityInstance()
    {
        if (animationCoroutine != null)
        {
            abilitySystem.StopCoroutine(animationCoroutine);
        }

        if (effectCoroutine != null)
        {
            abilitySystem.StopCoroutine(effectCoroutine);
        }
    }
}
