using UnityEngine;

[CreateAssetMenu(menuName="Gameplay Ability/Jump")]
public class GameplayAbility_Jump : GameplayAbility_Base
{
    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);
        
        PlayerCharacter playerCharacter = abilitySystem.avatar.GetComponent<PlayerCharacter>();
        if (playerCharacter != null)
        {
            playerCharacter.HandleJump();
        }
    }

    public override bool CanActivateAbility()
    {
        PlayerCharacter playerCharacter = abilitySystem.avatar.GetComponent<PlayerCharacter>();
        return playerCharacter.CanJump();
    }
}