using System.Collections;
using UnityEngine;

public abstract class GameplayAbilityTask_Base : MonoBehaviour
{   
    GameplayAbility_Base owningAbility;
    AbilitySystem owningSystem;

    public virtual void InitTask(GameplayAbility_Base executingAbility, AbilitySystem executingSystem)
    {
        owningAbility = executingAbility;
        owningSystem = executingSystem;
    }

    public abstract IEnumerator RunTask();

    public virtual void EndTask()
    {
        // Do nothing by default.
    }
}
