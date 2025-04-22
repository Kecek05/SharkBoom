using UnityEngine;

public class DebugAnimationEvent : MonoBehaviour
{
    public void DebugEvent(string  eventName)
    {
        Debug.Log($"Animation Event Triggered: {eventName}");
    }
}
