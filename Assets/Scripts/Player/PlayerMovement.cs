using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    // Delegates
    public delegate void SimpleMovementDelegate();
    public delegate void MovementStateChangedDelegate(SPlayerMovementState newState, SPlayerMovementState oldState);
    public SimpleMovementDelegate onJumpApexReached { get; set; }
    public MovementStateChangedDelegate onMovementStateChanged { get; set; }

    // Properties
    public Rigidbody playerRigidbody { get; private set; }
    public CapsuleCollider playerCapsule { get; private set; }
    public Vector3 inputVector { get; set; }
    public int currentNumJumps { get; private set; }
    public bool isCurrentlyJumping { get; private set; }
    public SPlayerMovementState activeMovementState { get; private set; }
    public SPlayerMovementState overrideMovementState { get; set; }

    #region EditorProperties
    [Header("Jump Settings")]
    [SerializeField] private int _maxJumps = 1;
    [SerializeField] private SPlayerMovementState _defaultMovementState;
    [SerializeField] private SPlayerJumpBehaviour _jumpBehaviour;
    [SerializeField] private SPlayerOrientationControl _playerOrientationOverride; 
    #endregion

    #region PropertyAccessors
    public int maxJumps { get { return _maxJumps; } set { _maxJumps = value; } }
    #endregion

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCapsule = GetComponent<CapsuleCollider>();
    }

    public bool CanJump()
    {
        return (maxJumps < 0 || currentNumJumps < maxJumps) && _jumpBehaviour != null && activeMovementState != null && activeMovementState.CanJump();
    }

    public bool IsJumpingOrFalling()
    {
        return activeMovementState != null && activeMovementState.IsFallingState();
    }

    public bool Jump()
    {
        if (!CanJump())
        {
            return false;
        }

        if (!_jumpBehaviour.ExecuteJump(this, playerRigidbody))
        {
            return false;
        }

        currentNumJumps += 1;
        isCurrentlyJumping = true;
        return true;
    }

    public void NotifyReachedJumpApex()
    {
        if (onJumpApexReached != null)
        {
            onJumpApexReached();
        }
    }

    private void FixedUpdate()
    {
        if (activeMovementState == null)
        {
            if (_defaultMovementState == null)
            {
                Debug.LogError($"No default movement state specified for {name}, unable to move.");
                return;
            }
            activeMovementState = _defaultMovementState;
            activeMovementState.EnterState();
        }

        if (activeMovementState != null)
        {
            SPlayerMovementState targetState = null;
            bool hasStateChangedThisFrame = false;

            if (overrideMovementState != null)
            {
                if (overrideMovementState != activeMovementState)
                {
                    activeMovementState.LeaveState();
                    activeMovementState = overrideMovementState;
                    activeMovementState.EnterState();
                    hasStateChangedThisFrame = true;
                }

                overrideMovementState = null;
            }

            if (!hasStateChangedThisFrame && activeMovementState.CheckShouldSwitchState(ref targetState))
            {
                if (targetState != null)
                {
                    activeMovementState.LeaveState();
                    activeMovementState = targetState;
                    activeMovementState.EnterState();
                }
            }

            activeMovementState.UpdateState(this, playerRigidbody);
        }

        if (_playerOrientationOverride != null)
        {
            _playerOrientationOverride.OrientPlayer(this);
        }
    }
}
