using System.Collections.Generic;
using Febucci.UI;
using UnityEngine;

public class DialogueManager : SingletonScriptBase<DialogueManager>
{
    public delegate void DialogueFinishedEvent(SDialogue dialogue);
    public DialogueFinishedEvent onDialogueFinished;
    public DialogueFinishedEvent onDialogueStarted;
    public System.Action onDialogueBarOpened;
    public System.Action onDialogueBarClosed;

    SDialogue _activeDialog;
    Queue<DialogueStep> _activeDialogueQueue = new Queue<DialogueStep>();

    UIDialogueBar _dialogueBar = null;

    public void RegisterDialogueBar(UIDialogueBar newDialogueBar)
    {
        _dialogueBar = newDialogueBar;
    }

    public void StartNewDialogue(SDialogue dialogue)
    {
        if (_dialogueBar == null)
        {
            Debug.LogError("Unable to run dialogues because no dialogue bar has been registered.");
        }

        _activeDialogueQueue.Clear();

        foreach (var step in dialogue.steps)
        {
            _activeDialogueQueue.Enqueue(step);
        }

        if (_activeDialogueQueue.Count > 0)
        {
            _activeDialog = dialogue;

            if (_dialogueBar.isOpen)
            {
                DisplayNextStep();
            }
            else
            {
                _dialogueBar.OpenDialogueWindow(OnDialogueBarOpened);
            }
        }
        else
        {
            if (!_dialogueBar.isClosed)
            {
                StopDialogue();
            }
        }
    }

    /// <summary>
    /// Intended to serve as an input notify for the manager. Should fast forward the current step if it has not 
    /// yet finished, or start the next step if it has.
    /// </summary>
    public void MoveDialogueForward()
    {
        if (_dialogueBar.isTyping)
        {
            _dialogueBar.SkipTypingForCurrentStep();
        }
        else if (!DisplayNextStep())
        {
            StopDialogue();
        }
    }

    public void StopDialogue()
    {
        _activeDialogueQueue.Clear();

        if (_activeDialog != null && onDialogueFinished != null)
        {
            onDialogueFinished(_activeDialog);
            _activeDialog = null;
        }

        _dialogueBar.CloseDialogueWindow();
    }

    bool DisplayNextStep()
    {
        if (_activeDialogueQueue.Count > 0)
        {
            DialogueStep nextStep = _activeDialogueQueue.Dequeue();
            _dialogueBar.ShowDialogueStep(nextStep, _activeDialogueQueue.Count == 0);

            return true;
        }

        return false;
    }

    void OnDialogueBarOpened()
    {
        if (!DisplayNextStep())
        {
            StopDialogue();
        }

        if (onDialogueBarOpened != null)
        {
            onDialogueBarOpened();
        }
    }

    void OnDialogueBarClosed()
    {
        if (onDialogueBarClosed != null)
        {
            onDialogueBarClosed();
        }
    }
}
