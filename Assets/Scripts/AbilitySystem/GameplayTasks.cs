using System.Collections;
using UnityEngine;
using Animancer;

namespace GameplayAbilitySystem
{
    public static class PlayAnimationAndWaitForEventTask
    {
        public struct AnimationEvent
        {
            public string key;
            public System.Action callback;
        }

        public static IEnumerator Start(HybridAnimancerComponent animator, ClipTransition animation, AnimationEvent[] events, System.Action onAnimationEnded)
        {
            if (animator != null && animation != null && animation.Transition != null)
            {
                if (events != null)
                {
                    foreach (AnimationEvent animEvent in events)
                    {
                        if (animation.Transition.Events.IndexOf(animEvent.key) != -1)
                        {
                            animation.Transition.Events.SetCallback(animEvent.key, animEvent.callback);
                        }
                    }
                }

                yield return animator.Play(animation.Transition);

                if (onAnimationEnded != null)
                {
                    onAnimationEnded();
                }

                if (events != null)
                {
                    foreach (AnimationEvent animEvent in events)
                    {
                        if (animation.Transition.Events.IndexOf(animEvent.key) != -1)
                        {
                            animation.Transition.Events.RemoveCallback(animEvent.key, animEvent.callback);
                        }
                    }
                }
            }
        }
    }

    public static class WaitEffectRemoved
    {
        public static IEnumerator Start(ActiveEffectHandle effect, System.Action onEffectRemoved)
        {
            while (effect.IsValid())
            {
                yield return null;
            }

            if (onEffectRemoved != null)
            {
                onEffectRemoved();
            }
        }
    }
}