using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Ability System/Gameplay Ability/Jump")]
public class SGameplayAbility_Jump : SGameplayAbility
{
    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);
        
        PlayerCharacter playerCharacter = abilitySystem.avatar.GetComponent<PlayerCharacter>();
        if (playerCharacter != null)
        {
            playerCharacter.HandleJump();
        }

        EndAbility(false);
    }

    public override bool CanActivateAbility()
    {
        PlayerCharacter playerCharacter = abilitySystem.avatar.GetComponent<PlayerCharacter>();
        return playerCharacter.CanJump();
    }
}