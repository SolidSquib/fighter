using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = "Ability System/Gameplay Ability/Player Melee")]
public class SGameplayAbility_PlayerMelee : SGameplayAbility
{
    [Header("Player Melee Properties")]
    public SGameplayEffect stopMovementEffect;
    public ClipState.Transition[] comboAnims;
    public float comboBufferTime = 0.2f;

    ActiveEffectHandle _stopMovementEffectHandle;
    HybridAnimancerComponent _animancer;
    SpriteAttachments _spriteAttachments;
    Coroutine _activeAnimationCoroutine;
    int _currentComboIndex = 0;
    Queue<float> _buttonPressTimestamps = new Queue<float>();
    bool _comboWindowOpen = false;

    public override void InitializeAbility()
    {
        _animancer = abilitySystem.GetComponentInChildren<HybridAnimancerComponent>();
        _spriteAttachments = abilitySystem.GetComponentInChildren<SpriteAttachments>();
 
        foreach (var anim in comboAnims)
        {
            if (anim != null && anim.Events.IndexOf("ComboWindowOpen") != -1)
            {
                anim.Events.SetCallback("ComboWindowOpen", OnComboWindowOpened);
            }

            if (anim != null && anim.Events.IndexOf("Damage") != -1)
            {
                anim.Events.SetCallback("Damage", OnDamageEvent);
            }
        }
    }

    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);

        _currentComboIndex = 0;
        StartNewAttackAnimation(false);

        if (stopMovementEffect != null)
        {
            GameplayEffectSpec spec = abilitySystem.MakeGameplayEffectSpec(stopMovementEffect);
            _stopMovementEffectHandle = abilitySystem.ApplyGameplayEffectSpecToSelf(spec);
        }
    }

    void StartNewAttackAnimation(bool incrementIndex)
    {
        if (_activeAnimationCoroutine != null)
        {
            abilitySystem.StopCoroutine(_activeAnimationCoroutine);
            _activeAnimationCoroutine = null;
        }

        _comboWindowOpen = false;
        _activeAnimationCoroutine = abilitySystem.StartCoroutine(PlayAnimation(incrementIndex ? IncrementComboIndex() : _currentComboIndex));
    }

    void OnComboWindowOpened()
    {
        while (_buttonPressTimestamps.Count > 0)
        {
            float inputTimestamp = _buttonPressTimestamps.Dequeue();
            if (Time.time - inputTimestamp <= comboBufferTime)
            {
                StartNewAttackAnimation(true);
                return;
            }
        }

        _comboWindowOpen = true;
    }

    void OnDamageEvent()
    {
        /* Transform attackOrigin;
        if (_spriteAttachments != null && _spriteAttachments.attachments.TryGetValue("AttackOrigin", out attackOrigin))
        {
            Vector3 amendedPosition = _spriteAttachments.transform.position;
            
            DebugExtension.DebugWireSphere(attackOrigin.position, Color.red, 10, 100,false);
        } */
    }

    public override void EndAbility(bool wasCancelled)
    {
        _buttonPressTimestamps.Clear();
        _animancer.Play(_animancer.Controller, 0); // Resumes the regular controller.
        abilitySystem.RemoveActiveEffectByHandle(_stopMovementEffectHandle);
        base.EndAbility(wasCancelled);
    }

    int IncrementComboIndex()
    {
        _currentComboIndex = (_currentComboIndex + 1) % comboAnims.Length;
        return _currentComboIndex;
    }

    IEnumerator PlayAnimation(int animIndex)
    {
        if (comboAnims.Length > animIndex)
        {
            ClipState.Transition animation = comboAnims[animIndex];
            yield return _animancer.Play(animation);
        }
        EndAbility(false);
    }

    public override void InputKeyDown()
    {
        base.InputKeyDown();
        if (_comboWindowOpen)
        {
            // Start a new attack here and close the combo window again.
            StartNewAttackAnimation(true);
        }
        else
        {
            _buttonPressTimestamps.Enqueue(Time.time);
        }
    }
}
