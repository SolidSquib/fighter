using UnityEngine;
using Fighter.Types;
using Fighter.Attributes;

[CreateAssetMenu(menuName="Audio Events/Simple")]
public class SimpleAudioEvent : AudioEvent_Base
{   
    public AudioClip[] clips;
    public RangedFloat volume;
    [MinMax(0,1,showEditRange=true)]
    public RangedFloat pitch;

    public override void Play(AudioSource source)
    {
        if (clips.Length <= 0)
        {
            Debug.LogWarning("An attempt was made to play a SimpleAudioEvent, but no clips were provided.");
            return;
        }

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }
}
