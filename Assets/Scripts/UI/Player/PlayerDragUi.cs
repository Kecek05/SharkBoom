using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDragUi : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private PlayerThrower player;
    [SerializeField] private LookAtCameraComponent lookAtCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        HideText(); //hide enemy ui
    }

    public void HandleOnPlayerDragControllerDragCancelable(bool isCancelable)
    {
        if(!IsOwner) return; //only owner

        if (isCancelable)
        {
            HideText();
        } else if(!isCancelable && player.PlayerStateMachine.CurrentState == player.PlayerStateMachine.draggingItem || player.PlayerStateMachine.CurrentState == player.PlayerStateMachine.draggingJump)
        {
            //cant cancell and its dragging
            ShowText();
        }

    }

    protected override void DoOnSpawn()
    {
        HideText();
    }


    public void HandleOnPlayerDragControllerDragStart()
    {
        if (!IsOwner) return; //only owner

        ShowText();
    }

    protected override void DoOnDragChange(float forcePercent, float angle)
    {
        forceText.text = $"Force: {Mathf.RoundToInt(forcePercent)}";
        directionText.text = $"Direction: {Mathf.RoundToInt(angle)}°";
    }

    protected override void DoOnDragRelease()
    {
        HideText();
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

}
