using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Movement/Orientation Control/Defaults/2D")]
public class S2DCharacterOrientationControl : SCharacterOrientationControl
{
    public List<SCharacterMovementState> allowedMovementStates = new List<SCharacterMovementState>();
    public bool projectInputVectorToCamera = false;

    protected override Vector3 GetLookAtDirection(CharacterMovement playerMovement)
    {
        Vector3 cameraRight = Camera.main.transform.right;
        float dot = Vector3.Dot(cameraRight, playerMovement.transform.forward);

        Vector3 inputVector = projectInputVectorToCamera ? new Vector3(Vector3.Dot(cameraRight, playerMovement.inputVector), 0f, 0f).normalized : playerMovement.inputVector;

        if (inputVector.x != 0 && allowedMovementStates.Contains(playerMovement.activeMovementState))
        {
            return Camera.main.transform.right * (inputVector.x > 0 ? 1 : -1);
        }
        else
        {
            return Camera.main.transform.right * (dot > 0 ? 1 : -1);
        }
    }
}
