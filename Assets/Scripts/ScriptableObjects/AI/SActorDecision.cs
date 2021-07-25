using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SActorDecision : ScriptableObject
{
    public abstract bool Decide(AIController controller);
}
