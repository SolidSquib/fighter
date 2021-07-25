using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SActorAction : ScriptableObject
{
    public abstract void Act(AIController controller);
}
