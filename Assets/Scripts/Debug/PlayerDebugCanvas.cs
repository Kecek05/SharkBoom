using Sortify;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDebugCanvas : NetworkBehaviour
{

    [BetterHeader("References")]
    public PlayerThrower player;
    public TextMeshProUGUI selectedItemIndexText;
    public TextMeshProUGUI selectedRbText;
    public TextMeshProUGUI playerStateText;
    public TextMeshProUGUI playerCanInteractWithInventoryText;
    public TextMeshProUGUI dragDistanceText;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

    }

    private void Update()
    {
        if (player.PlayerDragController.SelectedRb == null)
        {
            selectedRbText.text = "null";
        }
        else
        {
            selectedRbText.text = player.PlayerDragController.SelectedRb.ToString();

        }

        if(player.PlayerDragController != null) 
        {
            dragDistanceText.text = $"Drag Distance: {Mathf.Abs(Mathf.RoundToInt(player.PlayerDragController.DragDistance))} Last Drag Distance: {Mathf.Abs(Mathf.RoundToInt(player.PlayerDragController.LastDragDistance))}";
        }


        playerStateText.text = player.PlayerStateMachine.CurrentState.ToString();

        selectedItemIndexText.text = player.PlayerInventory.SelectedItemInventoryIndex.ToString();

        playerCanInteractWithInventoryText.text = player.PlayerInventory.CanInteractWithInventory.ToString();
    }



}
