using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instantiate an attribute set at runtime on the AbilitySystem script so that we can easily serialize and deserialize all 
/// relevant player attributes when required.
/// </summary>
[CreateAssetMenu(menuName = "Ability System/Attribute Set")]
public class SAttributeSet : ScriptableObject
{
    public delegate void AttributeDelegate(float value);

    private class AttributeInstance
    {
        public float currentValue { get; set; }
        public float baseValue { get; set; }
    }

    [SerializeField] private List<SAttribute> _attributes;

    private Dictionary<SAttribute, AttributeInstance> _currentAttributeValues;
    private Dictionary<SAttribute, AttributeDelegate> _attributeModifiedCallbacks;

    public float GetAttributeCurrentValue(SAttribute attribute)
    {
        AttributeInstance instance;
        if (_currentAttributeValues.TryGetValue(attribute, out instance))
        {
            return instance.currentValue;
        }
        return 0;
    }

    public float GetAttributeBaseValue(SAttribute attribute)
    {
        AttributeInstance instance;
        if (_currentAttributeValues.TryGetValue(attribute, out instance))
        {
            return instance.baseValue;
        }
        return 0;
    }

    public void RegisterOnAttributeChangedCallback(SAttribute attribute, AttributeDelegate callback)
    {
        if (callback == null)
        {
            return;
        }

        AttributeDelegate existingDelegate;
        if (_attributeModifiedCallbacks.TryGetValue(attribute, out existingDelegate))
        {
            existingDelegate += callback;
        }
        else
        {
            _attributeModifiedCallbacks.Add(attribute, callback);
        }
    }

    public void UnregisterOnAttributeChangedCallback(SAttribute attribute, AttributeDelegate callback)
    {
        if (callback == null)
        {
            return;
        }

        AttributeDelegate existingDelegate;
        if (_attributeModifiedCallbacks.TryGetValue(attribute, out existingDelegate))
        {
            existingDelegate -= callback;

            if (existingDelegate == null)
            {
                _attributeModifiedCallbacks.Remove(attribute);
            }
        }
    }

    /// <summary>
    /// Apply instant modifiers.
    /// </summary>
    /// <param name="modifier"></param>
    /// <param name="setByCallerTags"></param>
    public void ExecuteAttributeModifier(CachedEffectModifierMagnitude modifier)
    {
        AttributeInstance attributeInstance;
        if (_currentAttributeValues.TryGetValue(modifier.attribute, out attributeInstance))
        {
            switch (modifier.method)
            {
                case EModifierMethod.Add:
                    attributeInstance.baseValue += modifier.magnitude;
                    break;
                case EModifierMethod.Multiply:
                    attributeInstance.baseValue *= modifier.magnitude;
                    break;
                case EModifierMethod.Divide:
                    attributeInstance.baseValue /= modifier.magnitude;
                    break;
                case EModifierMethod.Override:
                    attributeInstance.baseValue = modifier.magnitude;
                    break;
            }
        }
    }
}
