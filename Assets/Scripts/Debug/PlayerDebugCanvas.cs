using Sortify;
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
    public PlayerDragController playerDragController;
    public PlayerInventory playerInventory;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Destroy(gameObject);
            return;
        }

    }

    private void Update()
    {
        if (playerDragController.SelectedRb == null)
        {
            selectedRbText.text = "null";
        }
        else
        {
            selectedRbText.text = playerDragController.SelectedRb.ToString();

        }

        if(playerDragController != null) 
        {
            dragDistanceText.text = $"Drag Distance: {Mathf.Abs(Mathf.RoundToInt(playerDragController.DragDistance))} Last Drag Distance: {Mathf.Abs(Mathf.RoundToInt(playerDragController.LastDragDistance))}";
        }


        playerStateText.text = player.PlayerStateMachine.CurrentState.ToString();

        selectedItemIndexText.text = playerInventory.SelectedItemInventoryIndex.ToString();

        playerCanInteractWithInventoryText.text = playerInventory.CanInteractWithInventory.ToString();
    }



}
