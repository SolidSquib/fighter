using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attack Combo")]
public class SAttackCombo : ScriptableObject
{
    public SAttack[] attacks;
    public bool allowLooping = true;
}
