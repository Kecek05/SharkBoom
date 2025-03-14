using Sortify;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDragUi : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI forceText;
    [SerializeField] private TextMeshProUGUI directionText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        HideText(); //hide enemy ui
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
        }
    }
}
