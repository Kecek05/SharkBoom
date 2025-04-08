using System.Collections;
using UnityEngine;

public abstract class BaseWarning : MonoBehaviour
{
    [SerializeField] protected GameObject warningPanel;
    protected WaitForSeconds waitForSeconds = new(2f); //Warning duration

    protected abstract void Start();
    protected virtual void StartWarning()
    {
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
