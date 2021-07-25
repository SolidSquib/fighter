using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

[CreateAssetMenu(menuName = "Character Data")]
public class SCharacterData : ScriptableObject
{
    [SerializeField] SDialogueSpeaker _speakerData;

    [Header ("Combos")]
    public SAttackCombo lightAttackCombo;
    public SAttackCombo heavyAttackCombo;

    [Header ("Animations")]
    public ClipTransition staggerAnimation;
    public ClipTransition stunnedAnimation;
}
