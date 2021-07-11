using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpeechBubble : MonoBehaviour
{
    public float heightOffset = 10.0f;

    public void PositionOverCharacter(PlayerCharacter character)
    {
        Vector3 characterTopWorld = character.transform.position + new Vector3(0f, character.GetComponent<CharacterController>().height / 2.0f, 0f);
        Vector3 characterTopScreen = Camera.main.WorldToScreenPoint(characterTopWorld);

        RectTransform speechBubbleRect = GetComponent<RectTransform>();

        Vector3 newBubbleLocation = new Vector3(
            characterTopScreen.x, 
            characterTopScreen.y + (speechBubbleRect.pivot.y * speechBubbleRect.rect.height) + heightOffset, 
            0f);

        transform.position = newBubbleLocation;
    }
}
