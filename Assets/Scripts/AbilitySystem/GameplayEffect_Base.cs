using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameplayEffect_Base : ScriptableObject
{
    public enum EEffectDurationPolicy { Instant, Infinite, Duration }
    
    public TagContainer effectTags = new TagContainer();
    public TagContainer grantedTags = new TagContainer();
    
}
