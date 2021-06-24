using System.Net.Mime;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputBuffering : MonoBehaviour
{
    void Awake()
    {
        GetComponent<PlayerInput>().onActionTriggered += HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "Jump")
        {
            //HandleJump();
        }
        Vector2 stickValue = context.ReadValue<Vector2>();
        Debug.Log(stickValue.ToString());
    }
}
