using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SCharacterOrientationControl : ScriptableObject
{
    protected abstract Vector3 GetLookAtDirection(CharacterMovement playerMovement);

    public void OrientPlayer(CharacterMovement playerMovement)
    {
        Vector3 lookAtDirection = GetLookAtDirection(playerMovement);
        playerMovement.transform.forward = lookAtDirection.normalized;        
    }
}
