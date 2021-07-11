using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability System/Gameplay Ability/Test Speech")]
public class STestSpeech : SGameplayAbility
{
    public SDialogue conversation;

    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);
        
        if (conversation == null)
        {
            Debug.LogWarning("Conversation property is null.");
            EndAbility(true);
        }

        DialogueManager.instance.onDialogueFinished += OnDialogueFinished;
        DialogueManager.instance.StartNewDialogue(conversation);
    }

    protected void OnDialogueFinished(SDialogue dialogue)
    {
        if (dialogue == conversation)
        {
            DialogueManager.instance.onDialogueFinished -= OnDialogueFinished;
            EndAbility(false);
        }
    }

    public override void InputKeyDown()
    {
        DialogueManager.instance.MoveDialogueForward();
    }
}
