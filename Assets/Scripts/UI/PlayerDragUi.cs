using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDragUi : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            player.PlayerDragController.OnDragCancelable += PlayerDragController_OnDragCancelable;
        }
        HideText(); //hide enemy ui
    }

    private void PlayerDragController_OnDragCancelable(bool isCancelable)
    {
        if(isCancelable)
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
        player.PlayerDragController.OnDragStart += PlayerDragController_OnDragStart;
    }


    private void PlayerDragController_OnDragStart()
    {
        ShowText();
    }

    protected override void DoOnDragChange()
    {
        forceText.text = "Force: " + Mathf.RoundToInt(player.PlayerDragController.GetForcePercentage());

        directionText.text = "Direction: " + Mathf.RoundToInt(player.PlayerDragController.GetAngle());
    }

    protected override void DoOnDragRelease()
    {
        HideText();
    }

    private void ShowText()
    {
        forceText.enabled = true;
        directionText.enabled = true;
    }

    private void HideText()
    {
        forceText.enabled = false;
        directionText.enabled = false;
    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if(IsOwner)
        {
            player.PlayerDragController.OnDragStart -= PlayerDragController_OnDragStart;
            player.PlayerDragController.OnDragCancelable -= PlayerDragController_OnDragCancelable;
        }
    }
}
