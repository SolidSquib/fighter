using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Data")]
public class SCharacterData : ScriptableObject
{
    [SerializeField] SDialogueSpeaker _speakerData;
}
