using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    public bool _startActive = true;

    private NavMeshAgent _navMeshAgent;
    private SActorState _currentState;
    private bool _aiActive = false;
    [SerializeField] private Transform _target;
    [SerializeField] private SActorState _remainState;
    [SerializeField] private SActorState _startState;

    public NavMeshAgent navMeshAgent { get { return _navMeshAgent; } private set { _navMeshAgent = value; } }
    public SActorState currentState { get { return _currentState; } private set { _currentState = value; } }
    public bool aiActive { get { return _aiActive; } private set { _aiActive = value; } }
    public Transform target { get { return _target; } set { _target = value; } }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        aiActive = _startActive;
        _currentState = _startState;
    }

    private void Update()
    {
        if (!aiActive)
        {
            return;
        }

        currentState.UpdateState(this);
    }

    public void TransitionToState(SActorState nextState)
    {
        if (nextState != _remainState)
        {
            currentState = nextState;
        }
    }

    private void OnDrawGizmos()
    {
        if (currentState != null)
        {
            Gizmos.color = currentState.debugColor;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
}
