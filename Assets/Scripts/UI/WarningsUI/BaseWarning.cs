using MoreMountains.Feedbacks;
using Sortify;
using System.Collections;
using UnityEngine;

public abstract class BaseWarning : MonoBehaviour
{
    [BetterHeader("Warning Settings")]
    [SerializeField] protected float warningDuration = 2f; //Warning duration
    [SerializeField] protected GameObject warningPanel;
    [SerializeField] protected MMF_Player warningFeel;
    protected WaitForSeconds waitForSeconds;

    protected abstract void Start();
    protected void StartWarning()
    {
        waitForSeconds = new WaitForSeconds(warningDuration); //Warning duration
        
        StartCoroutine(WarningCoroutine());
    }

    protected virtual IEnumerator WarningCoroutine()
    {
        ShowWarning();

        yield return waitForSeconds;

        HideWarning();
    }

    protected void ShowWarning()
    {
        warningPanel.gameObject.SetActive(true);
        warningFeel?.PlayFeedbacks();
    }

    protected void HideWarning()
    {
        warningPanel.gameObject.SetActive(false);
    }

    protected abstract void OnDestroy();
}
