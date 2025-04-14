using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    // This class is used to receive animation events from the Animator component.
    // It can be attached to any GameObject that has an Animator component.

    [SerializeField] List<AnimationEvent> animationEvents = new List<AnimationEvent>();

    public void OnAnimationEventTriggered(string eventName)
    {
        // This method is called by the Animator component when an animation event is triggered.
        // It finds the corresponding AnimationEvent and invokes its UnityEvent.

        AnimationEvent matchingEvent = animationEvents.Find(se => se.eventName == eventName);
        matchingEvent?.OnAnimationEvent.Invoke();
    }
}

