using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = "Scriptable Objects/Attack")]
public class SAttack : ScriptableObject
{
    public ClipTransition animation;
    public SGameplayEffect[] activeAttackerEffects;
    public SGameplayEffect[] hitReceiverEffects;
    public string hitZoneSocketName = "AttackOrigin";
    public float hitRadius = 0.5f;
}
