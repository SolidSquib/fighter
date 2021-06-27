using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIHealthBar : MonoBehaviour
{
    public Slider _slider;
    public SAttribute _maxHealthAttribute;
    public SAttribute _healthAttribute;
    public PlayerCharacter _character;

    private void OnEnable()
    {
        if (_character)
        {
            InitializeForCharacter(_character);
        }
    }

    private void OnDisable()
    {
        Uninitialize();
    }

    public void SetHealth(float newHealth)
    {
        _slider.value = newHealth;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        _slider.maxValue = newMaxHealth;
    }

    public void InitializeForCharacter(PlayerCharacter character)
    {
        if (character == null)
        {
            return;
        }

        Uninitialize();

        AbilitySystem abilitySystem = character.GetComponent<AbilitySystem>();
        if (abilitySystem != null)
        {
            if (_healthAttribute != null)
            {
                abilitySystem.RegisterOnAttributeChangedCallback(_healthAttribute, SetHealth);
            }

            if (_maxHealthAttribute != null)
            {
                abilitySystem.RegisterOnAttributeChangedCallback(_maxHealthAttribute, SetMaxHealth);
            }
        }

        _character = character;
    }

    protected void Uninitialize()
    {
        if (_character == null)
        {
            return;
        }

        AbilitySystem abilitySystem = _character.GetComponent<AbilitySystem>();
        if (abilitySystem != null)
        {
            if (_healthAttribute != null)
            {
                abilitySystem.UnregisterOnAttributeChangedCallback(_healthAttribute, SetHealth);
            }

            if (_maxHealthAttribute != null)
            {
                abilitySystem.UnregisterOnAttributeChangedCallback(_maxHealthAttribute, SetMaxHealth);
            }
        }

        _character = null;
    }
}
