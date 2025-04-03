using Sortify;
using TMPro;
using UnityEngine;

public class PlayerDragUi : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private PlayerThrower player;
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

}
