using Sortify;
using System.Collections;
using UnityEngine;

public abstract class BaseWarning : MonoBehaviour
{
    [BetterHeader("Warning Settings")]
    [SerializeField] protected float warningDuration = 2f; //Warning duration
    [SerializeField] protected GameObject warningPanel;
    protected WaitForSeconds waitForSeconds;

    protected abstract void Start();
    protected virtual void StartWarning()
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

    protected virtual void ShowWarning()
    {
        warningPanel.gameObject.SetActive(true);
    }

    protected virtual void HideWarning()
    {
        warningPanel.gameObject.SetActive(false);
    }

    protected abstract void OnDestroy();
}
