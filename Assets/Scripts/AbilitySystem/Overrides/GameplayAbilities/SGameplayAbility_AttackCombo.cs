using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = "Ability System/Gameplay Ability/Attack Combo")]
public class SGameplayAbility_AttackCombo : SGameplayAbility
{
    [Header("Attack Combo Properties")]
    public SAttackCombo combo;
    public float comboBufferTime = 0.2f;
    public bool drawAttackTrace = false;
    public SGameplayEffect[] abilityActivationEffects;

    List<ActiveEffectHandle> _activeAbilityEffectHandles;
    List<ActiveEffectHandle> _activeAttackEffectHandles;

    HybridAnimancerComponent _animancer;
    SpriteAttachments _spriteAttachments;
    Coroutine _activeAnimationCoroutine;
    int _currentComboIndex = 0;
    Queue<float> _buttonPressTimestamps = new Queue<float>();
    bool _comboWindowOpen = false;

    public override void InitializeAbility()
    {
        _activeAbilityEffectHandles = new List<ActiveEffectHandle>();
        _activeAttackEffectHandles = new List<ActiveEffectHandle>();

        _animancer = abilitySystem.GetComponentInChildren<HybridAnimancerComponent>();
        _spriteAttachments = abilitySystem.GetComponentInChildren<SpriteAttachments>();

        foreach (var attack in combo.attacks)
        {
            if (attack != null && attack.animation != null)
            {
                if (attack.animation.Transition.Events.IndexOf("ComboWindowOpen") != -1)
                {
                    attack.animation.Transition.Events.SetCallback("ComboWindowOpen", OnComboWindowOpened);
                }

                if (attack.animation.Transition.Events.IndexOf("DamageInstant") != -1)
                {
                    attack.animation.Transition.Events.SetCallback("DamageInstant", OnDamageEvent);
                }
            }
        }
    }

    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);

        _currentComboIndex = 0;
        StartNewAttackAnimation(false);

        foreach (var effect in abilityActivationEffects)
        {
            GameplayEffectSpec spec = abilitySystem.MakeGameplayEffectSpec(effect);
            _activeAbilityEffectHandles.Add(abilitySystem.ApplyGameplayEffectSpecToSelf(spec));
        }
    }

    public override void EndAbility(bool wasCancelled)
    {
        _buttonPressTimestamps.Clear();
        _animancer.Play(_animancer.Controller, 0); // Resumes the regular controller.

        foreach (var handle in _activeAbilityEffectHandles)
        {
            abilitySystem.RemoveActiveEffectByHandle(handle);
        }
        _activeAbilityEffectHandles.Clear();

        foreach (var handle in _activeAttackEffectHandles)
        {
            abilitySystem.RemoveActiveEffectByHandle(handle);
        }
        _activeAttackEffectHandles.Clear();

        base.EndAbility(wasCancelled);
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

    void StartNewAttackAnimation(bool incrementIndex)
    {
        if (_activeAnimationCoroutine != null)
        {
            abilitySystem.StopCoroutine(_activeAnimationCoroutine);
            _activeAnimationCoroutine = null;
        }

        _comboWindowOpen = false;

        foreach (var handle in _activeAttackEffectHandles)
        {
            abilitySystem.RemoveActiveEffectByHandle(handle);
        }
        _activeAttackEffectHandles.Clear();

        _activeAnimationCoroutine = abilitySystem.StartCoroutine(PlayAnimation(incrementIndex ? IncrementComboIndex() : _currentComboIndex));

        foreach (var effect in GetCurrentAttack().activeAttackerEffects)
        {
            GameplayEffectSpec spec = abilitySystem.MakeGameplayEffectSpec(effect);
            _activeAttackEffectHandles.Add(abilitySystem.ApplyGameplayEffectSpecToSelf(spec));
        }
    }

    void OnComboWindowOpened()
    {
        // Don't re-open the combo window if we are on the last attack and this combo doesn't allow looping.
        if (!combo.allowLooping && _currentComboIndex >= (combo.attacks.Length - 1))
        {
            return;
        }

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
        Transform attackOrigin;
        if (_spriteAttachments != null && _spriteAttachments.attachments.TryGetValue(GetCurrentAttack().hitZoneSocketName, out attackOrigin))
        {
            Vector3 amendedPosition = _spriteAttachments.transform.position;
            Collider[] meleeHits = Physics.OverlapSphere(attackOrigin.position, GetCurrentAttack().hitRadius);

            foreach (Collider col in meleeHits)
            {
                Debug.Log($"Melee hit on {col.gameObject.name}");
            }

            if (drawAttackTrace)
            {
                DebugExtension.DebugWireSphere(attackOrigin.position, Color.red, GetCurrentAttack().hitRadius, 2, false);
            }
        }
    }

    int IncrementComboIndex()
    {
        _currentComboIndex = (_currentComboIndex + 1) % combo.attacks.Length;
        return _currentComboIndex;
    }

    IEnumerator PlayAnimation(int attackIndex)
    {
        if (combo.attacks.Length > attackIndex)
        {
            ClipState.Transition animation = combo.attacks[attackIndex].animation.Transition;
            yield return _animancer.Play(animation);
        }
        EndAbility(false);
    }

    protected SAttack GetCurrentAttack()
    {
        return combo.attacks[_currentComboIndex];
    }
}
