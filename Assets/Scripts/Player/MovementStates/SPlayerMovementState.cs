using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SPlayerMovementState : ScriptableObject
{
    [SerializeField] private float _baseMoveSpeed = 10;

    public float baseMoveSpeed { get { return _baseMoveSpeed; } protected set { _baseMoveSpeed = value; } }

    public virtual bool CanJump() { return false; }
    public virtual bool IsFallingState() { return false; }
    public virtual void EnterState() {}
    public abstract void UpdateState(PlayerMovement playerMovement, Rigidbody playerRigidbody);
    public virtual void LeaveState() {}

    public virtual bool CheckShouldSwitchState(ref SPlayerMovementState newState) { return false; }
}
