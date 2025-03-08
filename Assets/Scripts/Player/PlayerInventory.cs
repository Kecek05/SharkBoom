using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<ItemDataStruct> OnItemAdded;
    public event Action<ItemDataStruct> OnItemChanged;
    public event Action<int> OnItemSelected;

    [SerializeField] private Player player;

    [SerializeField] private ItemsListSO itemsListSO;

    private NetworkList<ItemDataStruct> playerInventory = new();

    /// <summary>
    /// The index of the selected item in the player inventory
    /// </summary>
    private NetworkVariable<int> selectedItemInventoryIndex = new(-1); //-1 to start change to 0 and Call OnValueChanged

    public NetworkVariable<int> SelectedItemInventoryIndex => selectedItemInventoryIndex;


    private bool canInteractWithInventory = false;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {

            playerInventory.OnListChanged += PlayerInventory_OnListChanged;

            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

            selectedItemInventoryIndex.OnValueChanged += SelectedItemIndex_OnValueChanged;

        }
    }

    private void PlayerStateMachine_OnStateChanged(IState state)
    {
        if (state == player.PlayerStateMachine.myTurnStartedState)
        {
            SetPlayerJumpedRpc(true); //can jump
            SetCanInteractWithInventory(true);
            SelectItemDataByItemInventoryIndex(); //Default to Jump
        }
        else if (state == player.PlayerStateMachine.idleMyTurnState)
        {
            SetCanInteractWithInventory(true);
        }
        else if (state == player.PlayerStateMachine.draggingJump || state == player.PlayerStateMachine.draggingItem)
        {
            SetCanInteractWithInventory(false);
        }
        else if (state == player.PlayerStateMachine.dragReleaseItem)
        {
            SetCanInteractWithInventory(false);

            //DecreaseAllItemsCooldownRpc();
            //UseItemByInventoryIndexRpc(selectedItemInventoryIndex.Value);
            //SelectItemDataByItemInventoryIndex();
        }
        else if (state == player.PlayerStateMachine.dragReleaseJump)
        {
            SetCanInteractWithInventory(false);

            //Jumped, can shoot
            SetPlayerJumpedRpc(false);
        }
        else if (state == player.PlayerStateMachine.myTurnEndedState)
        {
            SetCanInteractWithInventory(false);


            DecreaseAllItemsCooldownRpc();
            UseItemByInventoryIndexRpc(selectedItemInventoryIndex.Value);
            SelectItemDataByItemInventoryIndex();
        }

    }

    [Rpc(SendTo.Server)]
    private void SetPlayerJumpedRpc(bool canJump)
    {
        playerInventory[0] = new ItemDataStruct
        {
            itemInventoryIndex = playerInventory[0].itemInventoryIndex, //first in inventory
            itemSOIndex = playerInventory[0].itemSOIndex, //first in itemsListSO
            itemCooldownRemaining = GetItemSOByItemSOIndex(playerInventory[0].itemSOIndex).cooldown, // No cooldown
            itemCanBeUsed = canJump, // if jumped, cant jump
        };
    }

    [Rpc(SendTo.Server)]
    public void DecreaseAllItemsCooldownRpc()
    {
        for (int i = 0; i < playerInventory.Count; i++)
        {
            if (!playerInventory[i].itemCanBeUsed)
            {
                playerInventory[i] = new ItemDataStruct
                {
                    itemInventoryIndex = playerInventory[i].itemInventoryIndex,
                    itemSOIndex = playerInventory[i].itemSOIndex,
                    itemCooldownRemaining = playerInventory[i].itemCooldownRemaining - 1,
                    itemCanBeUsed = playerInventory[i].itemCooldownRemaining - 1 <= 0, // if less or equal than 0, can be used
                };
            }
        }
    }

    private int SelectFirstItemInventoryIndexAvailable()
    {
        for (int i = 0; i < playerInventory.Count; i++)
        {
            if (playerInventory[i].itemCanBeUsed)
            {
                Debug.Log($"Item {i} is available");
                return i;
            }
        }
        Debug.LogWarning("No item available");
        return -1;
    }

    private void PlayerInventory_OnListChanged(NetworkListEvent<ItemDataStruct> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<ItemDataStruct>.EventType.Add:
                if (changeEvent.Value.itemInventoryIndex == 0) return; //Dont add item UI on Jump, index 0 is jump
                OnItemAdded?.Invoke(changeEvent.Value);
                break;
            case NetworkListEvent<ItemDataStruct>.EventType.Value:
                OnItemChanged?.Invoke(changeEvent.Value);

                if(changeEvent.Value.itemInventoryIndex == 0) //its jump
                {
                    if(!changeEvent.Value.itemCanBeUsed)
                    {
                        //Jumped just now, select other item
                        SelectItemDataByItemInventoryIndex(SelectFirstItemInventoryIndexAvailable());
                        Debug.Log("Jumped, select other item");
                    }
                }
                break;
        }
    }


    public void SetPlayerItems(int itemSOIndex) //Set the items that player have when starting the game
    {

        playerInventory.Add(new ItemDataStruct
        {
            itemInventoryIndex = playerInventory.Count, //get the index
            itemSOIndex = itemSOIndex,
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });
    }

    public void SelectItemDataByItemInventoryIndex(int itemInventoryIndex = 0) // Select a item to use, UI will call this, default (0) its Jump
    {

        if(!canInteractWithInventory) return;


        if (!ItemCanBeUsed(itemInventoryIndex))
        {
            Debug.LogWarning("Item can't be used!");
            return;
        }

        SetSelectedItemIndexRpc(itemInventoryIndex);

    }

    [Rpc(SendTo.Server)]
    private void SetSelectedItemIndexRpc(int itemInventoryIndex)
    {
        selectedItemInventoryIndex.Value = itemInventoryIndex;

    }

    private void SelectedItemIndex_OnValueChanged(int previousValue, int newValue)
    {
        //Owner only
        // Need to be an OnValueChanged event because the lag between the client and the server

        player.PlayerDragController.SetDragAndShoot(GetSelectedItemSO().rb);

        OnItemSelected?.Invoke(selectedItemInventoryIndex.Value);

        Debug.Log($"Selected Item Index: {newValue}");

    }

    [Rpc(SendTo.Server)]
    public void UseItemByInventoryIndexRpc(int itemInventoryIndex) // Use the item, Server will call this when both players ready
    {
        if (!canInteractWithInventory) return;

        if (ItemCanBeUsed(itemInventoryIndex))
        {
            //Item Can be used

            playerInventory[itemInventoryIndex] = new ItemDataStruct
            {
                itemInventoryIndex = playerInventory[itemInventoryIndex].itemInventoryIndex, //do not lose the index
                itemSOIndex = playerInventory[itemInventoryIndex].itemSOIndex,
                itemCooldownRemaining = GetItemSOByItemSOIndex(playerInventory[itemInventoryIndex].itemSOIndex).cooldown,
                itemCanBeUsed = false,
            };

        } else
        {
            Debug.LogWarning("Item can't be used!");
        }
    }

    public bool ItemCanBeUsed(int itemInventoryIndex) // Returns if the item can be used
    {

        return playerInventory[itemInventoryIndex].itemCanBeUsed;
            
    }

    public ItemSO GetSelectedItemSO()
    {
        return GetItemSOByItemSOIndex(playerInventory[selectedItemInventoryIndex.Value].itemSOIndex);
    }

    public ItemSO GetItemSOByItemSOIndex(int itemSOIndex)
    {
        return itemsListSO.allItemsSOList[itemSOIndex];
    }

    private void SetCanInteractWithInventory(bool canInteract)
    {
        canInteractWithInventory = canInteract;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInventory.OnListChanged -= PlayerInventory_OnListChanged;

            selectedItemInventoryIndex.OnValueChanged -= SelectedItemIndex_OnValueChanged;

            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
        }
    }
}
