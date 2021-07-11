using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Febucci.UI;
using Animancer;

public class UIDialogueBar : MonoBehaviour
{
    enum ETransitionState { Open, Closed, Opening, Closing }

    [SerializeField] Image _portrait;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _dialogueText;
    [SerializeField] TextAnimatorPlayer _textAnimator;
    [SerializeField] Image _advancementIndicator;
    [SerializeField] Image _closeIndicator;

    [Header("Animations")]
    [SerializeField] ClipState.Transition _openDialogueBarAnimation;
    [SerializeField] ClipState.Transition _closeDialogueBarAnimation;

    Animator _animator;
    AnimancerComponent _animancer;

    bool _isTyping = false;
    bool _isFinalStep = false;

    ETransitionState _transitionState = ETransitionState.Closed;
    DialogueStep _currentDialogueStep;

    UnityAction _currentOpenedCallback;
    UnityAction _currentClosedCallback;
    public System.Action finishedTypingCallback;

    public bool isTyping { get { return _isTyping; } }
    public bool isOpen { get { return _transitionState == ETransitionState.Open; } }
    public bool isClosed { get { return _transitionState == ETransitionState.Closed; } }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animancer = GetComponent<AnimancerComponent>();

        DialogueManager.instance.RegisterDialogueBar(this);

        _textAnimator.onTextShowed.AddListener(OnTypewriterFinishedTyping);
        _textAnimator.onTypewriterStart.AddListener(OnTypewriterStartedTyping);

        _nameText.text = "";
        _dialogueText.text = "";
        _portrait.sprite = null;
        _portrait.color = Color.clear;
    }

    public void OpenDialogueWindow(System.Action optionalCallback = null)
    {
        if (_transitionState == ETransitionState.Open)
        {
            return;
        }

        if (optionalCallback != null)
        {
            _openDialogueBarAnimation.Events.OnEnd += optionalCallback;
        }

        if (!_animancer.IsPlaying(_openDialogueBarAnimation))
        {
            _openDialogueBarAnimation.Events.OnEnd += DialogueTransitionComplete;
            _animancer.Play(_openDialogueBarAnimation);
        }

        _transitionState = ETransitionState.Opening;
        ShowAdvancementIndicator(false);
    }

    public void CloseDialogueWindow(System.Action optionalCallback = null)
    {
        if (_transitionState == ETransitionState.Closed)
        {
            return;
        }

        if (optionalCallback != null)
        {
            _closeDialogueBarAnimation.Events.OnEnd += optionalCallback;
        }

        if (!_animancer.IsPlaying(_closeDialogueBarAnimation))
        {
            _closeDialogueBarAnimation.Events.OnEnd += DialogueTransitionComplete;
            _animancer.Play(_closeDialogueBarAnimation);
        }

        _transitionState = ETransitionState.Closing;
        ShowAdvancementIndicator(false);
    }

    void DialogueTransitionComplete()
    {
        if (_transitionState == ETransitionState.Opening)
        {
            if (_currentOpenedCallback != null)
            {
                _currentOpenedCallback();
                _currentOpenedCallback = null;
            }

            _openDialogueBarAnimation.Events.OnEnd = null;
        }
        else if (_transitionState == ETransitionState.Closing)
        {
            if (_currentClosedCallback != null)
            {
                _currentClosedCallback();
                _currentClosedCallback = null;
            }

            _closeDialogueBarAnimation.Events.OnEnd = null;
            _nameText.text = "";
            _dialogueText.text = "";
            _portrait.sprite = null;
            _portrait.color = Color.clear;

            ShowAdvancementIndicator(false);
        }
    }

    public void ShowDialogueStep(DialogueStep dialogueStep, bool isFinalStep = false, System.Action finishedTypingCallback = null)
    {
        _nameText.text = dialogueStep.speaker.speakerName;
        _nameText.color = dialogueStep.speaker.color;
        _portrait.sprite = dialogueStep.speaker.portrait;
        _portrait.color = Color.white;
        _currentDialogueStep = dialogueStep;
        _textAnimator.ShowText(dialogueStep.dialogue);

        _isFinalStep = isFinalStep;
    }

    public void SkipTypingForCurrentStep()
    {
        _textAnimator.SkipTypewriter();
    }

    protected void OnTypewriterStartedTyping()
    {
        _isTyping = true;
        ShowAdvancementIndicator(false);
    }

    protected void OnTypewriterFinishedTyping()
    {
        _isTyping = false;
        
        ShowAdvancementIndicator(true);

        if (finishedTypingCallback != null)
        {
            finishedTypingCallback();
            finishedTypingCallback = null;
        }
    }

    protected void ShowAdvancementIndicator(bool show)
    {
        if (_advancementIndicator != null)
        {
            _advancementIndicator.gameObject.SetActive(show && !_isFinalStep);
        }

        if (_closeIndicator != null)
        {
            _closeIndicator.gameObject.SetActive(show && _isFinalStep);
        }
    }
}
