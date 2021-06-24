using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Jump Behaviour/Defaults/No Momentum")]
public class SBasicPlayerJump_NoMomentum : SPlayerJumpBehaviour
{
    public override bool ExecuteJump(PlayerMovement playerMovement, Rigidbody playerRigidbody)
    {
        if (playerRigidbody == null)
        {
            return false;
        }        
        
        playerRigidbody.velocity = new Vector3(0, jumpVelocity, 0); // Set initial jump velocity
        return true;
    }
}