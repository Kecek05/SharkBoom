using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDragUi : DragListener, IDetectDragStart, IDetectDragChange, IDetectDragRelease, IDetectDragCancelable, IDetectEndedTurn
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private LookAtCameraComponent lookAtCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        HideText(); //hide enemy ui
    }

    public void DoOnDragStart()
    {
        ShowText();
    }

    public void DoOnDragCancelable(bool cancelable)
    {
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

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        forceText.text = $"Force: {Mathf.RoundToInt(forcePercent)}";
        directionText.text = $"Direction: {Mathf.RoundToInt(andlePercent)}°";
    }

    private void ShowText()
    {
        forceText.enabled = true;
        directionText.enabled = true;
        lookAtCamera.enabled = true; // we enable and disable because this script work on LateUpdate
    }

    private void HideText()
    {
        forceText.enabled = false;
        directionText.enabled = false;
        lookAtCamera.enabled = false;
    }

    public void DoOnDragRelease()
    {
        HideText();
    }

    public void DoOnEndedTurn()
    {
        HideText();
    }
}
