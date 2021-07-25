using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorStateTransition
{
    public SActorDecision decision;
    public SActorState trueState;
    public SActorState falseState;
}

[CreateAssetMenu(menuName = "AI Actor/State")]
public class SActorState : ScriptableObject
{
    [SerializeField] private SActorAction[] _actions;
    [SerializeField] private ActorStateTransition[] _transitions;
    [SerializeField] private Color _debugColor = Color.grey;

    public SActorAction[] actions { get { return _actions; } private set { _actions = value; } }
    public ActorStateTransition[] transitions { get { return _transitions; } private set { _transitions = value; } }
    public Color debugColor { get { return _debugColor; } private set { _debugColor = value; } }

    public void UpdateState(AIController controller)
    {
        Act(controller);
        CheckTransitions(controller);
    }

    private void Act(AIController controller)
    {
        foreach (var action in _actions)
        {
            action.Act(controller);
        }
    }

    private void CheckTransitions(AIController controller)
    {
        foreach (var transition in transitions)
        {
            bool decisionValue = transition.decision.Decide(controller);
            controller.TransitionToState(decisionValue ? transition.trueState : transition.falseState);
        }
    }
}
