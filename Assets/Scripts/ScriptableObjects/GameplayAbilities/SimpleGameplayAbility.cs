using UnityEngine;

[CreateAssetMenu(menuName="Gameplay Ability/Simple")]
public class SimpleGameplayAbility : GameplayAbility_Base
{
    public override void ActivateAbility(GameplayEventData payload)
    {
        Debug.Log("Activated");     
    }
}