using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueStep
{
    public SDialogueSpeaker speaker;
    [TextArea(1, 3)]
    public string dialogue;
}

[CreateAssetMenu(menuName = "Dialogue/Conversation")]
public class SDialogue : ScriptableObject
{
    public DialogueStep[] steps;
}
