using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SAttribute : ScriptableObject
{
    public abstract float ModifyAttributeBase(float magnitude, float baseValue);
    public abstract float CalculateAttributeCurrentValue(List<float> modifierMagnitudes, float baseValue);
}

/// <summary>
/// Instantiate an attribute set at runtime on the AbilitySystem script so that we can easily serialize and deserialize all 
/// relevant player attributes when required.
/// </summary>
public abstract class AttributeSet_Base : ScriptableObject
{
    private class AttributeInstance
    {
        private List<float> _activeModifiers = new List<float>();

        public float currentValue { get; set; }
        public float baseValue { get; set; }
        public List<float> activeModifiers { get { return _activeModifiers; } set { _activeModifiers = value; } }
    }

    [SerializeField] private List<SAttribute> _attributes;

    private Dictionary<SAttribute, AttributeInstance> _currentAttributeValues { get; set; }

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
}
