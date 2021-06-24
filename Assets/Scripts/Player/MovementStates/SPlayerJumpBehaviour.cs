using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SPlayerJumpBehaviour : ScriptableObject
{
    public float jumpVelocity = 100.0f;

    public abstract bool ExecuteJump(PlayerMovement playerMovement, Rigidbody playerRigidbody);
}