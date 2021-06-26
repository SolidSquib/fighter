using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay Ability/Dash")]
public class SGameplayAbility_Dash : GameplayAbility_Base
{
    public override void ActivateAbility(GameplayEventData payload)
    {
        base.ActivateAbility(payload);

        Debug.Log("Jello Whirled");
    }

    public override void EndAbility(bool wasCancelled)
    {
        Debug.Log("Goodbye sweet world");
        base.EndAbility(wasCancelled);
    }

    public override void InputKeyUp()
    {
        EndAbility(false);
    }
}
