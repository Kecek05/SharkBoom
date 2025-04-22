using UnityEngine;

public class AnimationEventStateBehaviour : StateMachineBehaviour
{
    /// <summary>
    /// Unique event name.
    /// </summary>
    public string eventName;

    /// <summary>
    /// Percentage of the animation time when the event should be triggered.
    /// </summary>
    [Tooltip("In % of the animation time when the event should be triggered")]
    [Range(0, 1f)] 
    public float triggerTime;

    private bool hasTriggered;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // save the current time of the animation.
        //stateInfo = 2.5 = 2 full loops + 50% of the third loop.
        // currentTime return the progress of the current loop, regardless how many times the animation has repeated.
        float currentTime = stateInfo.normalizedTime % 1f;

        if(!hasTriggered && currentTime >= triggerTime)
        {
            hasTriggered = true;
            NotifyRefeiver(animator);
        }
    }

    private void NotifyRefeiver(Animator animator)
    {
        AnimationEventReceiver receiver = animator.GetComponent<AnimationEventReceiver>();
        if(receiver != null)
        {
            receiver.OnAnimationEventTriggered(eventName);
        }
        else
        {
            Debug.LogWarning($"No AnimationEventReceiver found on {animator.name}");
        }
    }

}
