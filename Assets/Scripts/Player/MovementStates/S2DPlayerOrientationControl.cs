using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement/Orientation Control/Defaults/2D")]
public class S2DPlayerOrientationControl : SPlayerOrientationControl
{
    public List<SPlayerMovementState> allowedMovementStates = new List<SPlayerMovementState>();

    protected override Vector3 GetLookAtDirection(PlayerMovement playerMovement)
    {
        Vector3 cameraRight = Camera.main.transform.right;
        float dot = Vector3.Dot(cameraRight, playerMovement.transform.forward);
        if (playerMovement.inputVector.x != 0 && allowedMovementStates.Contains(playerMovement.activeMovementState))
        {
            return Camera.main.transform.right * (playerMovement.inputVector.x > 0 ? 1 : -1);
        }
        else
        {
            return Camera.main.transform.right * (dot > 0 ? 1 : -1);
        }
    }
}
