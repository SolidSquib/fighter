using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SCharacterMovementState : ScriptableObject
{
    [SerializeField] private float _baseMoveSpeed = 10;

    public float baseMoveSpeed { get { return _baseMoveSpeed; } protected set { _baseMoveSpeed = value; } }

    public virtual bool CanJump() { return false; }
    public virtual bool IsFallingState() { return false; }
    public virtual void EnterState(CharacterMovement characterMovement) {}
    public abstract void UpdateState(CharacterMovement characterMovement, Vector3 inputVelocity);
    public virtual void LeaveState(CharacterMovement characterMovement) {}

    public virtual bool CheckShouldSwitchState(CharacterMovement characterMovement, ref SCharacterMovementState newState) { return false; }
}
