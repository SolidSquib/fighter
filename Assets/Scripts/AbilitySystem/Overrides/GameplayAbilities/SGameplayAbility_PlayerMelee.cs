using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = "Ability System/Gameplay Ability/Player Melee")]
public class SGameplayAbility_PlayerMelee : SGameplayAbility
{
    public SGameplayEffect stopMovementEffect;
    public AnimationClip attackAnim;

    ActiveEffectHandle _stopMovementEffectHandle;

    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);

        Animator animator = abilitySystem.animator;
        animator.SetTrigger("Attack");

        AnimancerComponent animancer = abilitySystem.GetComponentInChildren<AnimancerComponent>();
        var state = animancer.Play(attackAnim);
        state.Events.OnEnd = OnAnimationEnd;

        //abilitySystem.StartCoroutine(EndAfterDelay(attackAnim.length));        

        if (stopMovementEffect != null)
        {
            GameplayEffectSpec spec = abilitySystem.MakeGameplayEffectSpec(stopMovementEffect);
            _stopMovementEffectHandle = abilitySystem.ApplyGameplayEffectSpecToSelf(spec);
        }
    }

    public override void EndAbility(bool wasCancelled)
    {
        base.EndAbility(wasCancelled);

        abilitySystem.RemoveActiveEffectByHandle(_stopMovementEffectHandle);
    }

    void OnAnimationEnd()
    {
        EndAbility(false);
    }

    IEnumerator EndAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Animator animator = abilitySystem.animator;
        animator.Play("Idle");
        EndAbility(false);
    }

    public override void InputKeyDown()
    {
        base.InputKeyDown();
    }

    public override void InputKeyUp()
    {
        base.InputKeyUp();
    }
}
