using UnityEngine;

public abstract class AudioEvent_Base : ScriptableObject
{
    public abstract void Play(AudioSource source);
}
