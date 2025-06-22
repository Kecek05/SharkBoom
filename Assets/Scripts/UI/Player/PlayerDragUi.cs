using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDragUi : DragListener, IDetectDragChange, IDetectDragRelease, IDetectDragCancelable, IDetectEndedTurn, IDetectIdleMyTurn
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private GameObject background;
    [SerializeField] private LookAtCameraComponent lookAtCamera;
    [SerializeField] private GameObject leftSocket;
    [SerializeField] private GameObject rightSocket;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        HideText(); //hide enemy ui
    }

    public void DoOnDragCancelable(bool cancelable)
    {
        Debug.Log($"DRAG UI CANCELABLE CHANGED TO: {cancelable}");
        if (cancelable)
        {
            HideText();
        }
        else
        {
            //cant cancell and its dragging
            ShowText();
        }
    }

    public void HandleOnRotationChanged(bool isRight)
    {
        if (isRight)
        {
            background.transform.position = rightSocket.transform.position;
        }
        else
        {
            background.transform.position = leftSocket.transform.position;
        }
    }

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        forceText.text = Mathf.RoundToInt(forcePercent).ToString();
        directionText.text = Mathf.RoundToInt(andlePercent).ToString();
    }

    private void ShowText()
    {
        Debug.Log($"DRAG UI ON");
        forceText.enabled = true;
        directionText.enabled = true;
        lookAtCamera.enabled = true; // we enable and disable because this script work on LateUpdate
        background.SetActive(true);
    }

    private void HideText()
    {
        Debug.Log($"DRAG UI OFF");
        forceText.enabled = false;
        directionText.enabled = false;
        lookAtCamera.enabled = false;
        background.SetActive(false);
    }

    public void DoOnDragRelease()
    {
        HideText();
    }

    public void DoOnEndedTurn()
    {
        HideText();
    }

    public void DoOnIdleMyTurn()
    {
        HideText();
    }
}
