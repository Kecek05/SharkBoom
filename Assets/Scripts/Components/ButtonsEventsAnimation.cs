using MoreMountains.Feedbacks;
using UnityEngine;

public class ButtonsEventsAnimation : MonoBehaviour
{
    [SerializeField] private MMFeedbacks clickFeedback;
    [SerializeField] private MMFeedbacks holdFeedback;
    [SerializeField] private MMFeedbacks releaseFeedback;

    public void OnClickFeedback()
    {
        clickFeedback?.PlayFeedbacks();
    }

    public void OnHoldFeedback()
    {
        holdFeedback?.PlayFeedbacks();
    }

    public void OnReleaseFeedback()
    {
        releaseFeedback?.PlayFeedbacks();
    }
}
