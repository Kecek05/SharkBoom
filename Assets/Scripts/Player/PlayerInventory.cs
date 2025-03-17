using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<ItemInventoryData> OnItemAdded;
    public event Action<ItemInventoryData> OnItemChanged;
    public event Action<int> OnItemSelected;

    [SerializeField] private Player player;

    [SerializeField] private ItemsListSO itemsListSO;

    private NetworkList<ItemInventoryData> playerInventory = new();

    /// <summary>
    /// The index of the selected item in the player's inventory
    /// </summary>
    private int selectedItemInventoryIndex = 0;

    public int SelectedItemInventoryIndex => selectedItemInventoryIndex;


    private bool canInteractWithInventory = false;

    public bool CanInteractWithInventory => canInteractWithInventory; //DEBUG

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            //Default jump on Spawn

            player.PlayerDragController.SetDragRb(GetItemSOByItemSOIndex(0).rb); //get jump's rb

            OnItemSelected?.Invoke(selectedItemInventoryIndex);


            playerInventory.OnListChanged += PlayerInventory_OnListChanged;

            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

            player.PlayerLauncher.OnItemLaunched += PlayerLauncher_OnItemLaunched;

        }
    }

    private void PlayerLauncher_OnItemLaunched(int itemInventoryIndex)
    {
        //item Launched
        if(itemInventoryIndex == 0) //Jumped
        {
            SetSelectedItemInventoryIndex(SelectFirstItemInventoryIndexAvailable(1)); // direct on Set to ignore canInteractWithInventory
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState state)
    {
        if (state == player.PlayerStateMachine.myTurnStartedState)
        {
            SetCanInteractWithInventory(true);

            if(!ItemCanBeUsed(selectedItemInventoryIndex)) // If selected item can't be used, select other one
                SelectItemDataByItemInventoryIndex(SelectFirstItemInventoryIndexAvailable());
        }
        else if (state == player.PlayerStateMachine.idleMyTurnState)
        {
            SetCanInteractWithInventory(true);
        }
        else if (state == player.PlayerStateMachine.idleEnemyTurnState)
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

        }
        else if (state == player.PlayerStateMachine.dragReleaseJump)
        {
            SetCanInteractWithInventory(false);

            //Jumped, can shoot
            SetPlayerCanJumpRpc(false);
        }
        else if (state == player.PlayerStateMachine.myTurnEndedState)
        {
            SetCanInteractWithInventory(false);

            DecreaseAllItemsCooldownRpc();
            UseItemByInventoryIndexRpc(selectedItemInventoryIndex);

            SetPlayerCanJumpRpc(true); //can jump, set before next round to be able to select
        } else if (state == player.PlayerStateMachine.playerGameOverState)
        {
            SetCanInteractWithInventory(false);

        }

    }

    [Rpc(SendTo.Server)]
    private void SetPlayerCanJumpRpc(bool canJump)
    {
        playerInventory[0] = new ItemInventoryData
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
                playerInventory[i] = new ItemInventoryData
                {
                    itemInventoryIndex = playerInventory[i].itemInventoryIndex,
                    itemSOIndex = playerInventory[i].itemSOIndex,
                    itemCooldownRemaining = playerInventory[i].itemCooldownRemaining - 1,
                    itemCanBeUsed = playerInventory[i].itemCooldownRemaining - 1 <= 0, // if less or equal than 0, can be used
                };
            }
        }
    }

    public int SelectFirstItemInventoryIndexAvailable(int startingIndex = 0) //can pass a index to ignore previously itens
    {
        for (int i = startingIndex; i < playerInventory.Count; i++)
        {
            if (playerInventory[i].itemCanBeUsed)
            {
                return i;
            }
        }
        Debug.LogWarning("No item available");
        return -1;
    }

    private void PlayerInventory_OnListChanged(NetworkListEvent<ItemInventoryData> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<ItemInventoryData>.EventType.Add:
                if (changeEvent.Value.itemInventoryIndex == 0) return; //Dont add item UI on Jump, index 0 is jump
                OnItemAdded?.Invoke(changeEvent.Value);
                break;
            case NetworkListEvent<ItemInventoryData>.EventType.Value:
                OnItemChanged?.Invoke(changeEvent.Value);
                break;
        }
    }


    public void SetPlayerItems(int itemSOIndex) //Set the items that player have when starting the game
    {
        //Server Code
        playerInventory.Add(new ItemInventoryData
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
            Debug.LogWarning("Item can't be selected!");
            return;
        }

        SetSelectedItemInventoryIndex(itemInventoryIndex);

    }

    [Rpc(SendTo.Server)]
    public void UseItemByInventoryIndexRpc(int itemInventoryIndex) // Use the item, Server will call this when both players ready
    {

        if (ItemCanBeUsed(itemInventoryIndex))
        {
            //Item Can be used

            playerInventory[itemInventoryIndex] = new ItemInventoryData
            {
                itemInventoryIndex = playerInventory[itemInventoryIndex].itemInventoryIndex, //do not lose the index
                itemSOIndex = playerInventory[itemInventoryIndex].itemSOIndex,
                itemCooldownRemaining = GetItemSOByItemSOIndex(playerInventory[itemInventoryIndex].itemSOIndex).cooldown,
                itemCanBeUsed = false,
            };

        } else
        {
            Debug.LogWarning("Used an item that can't be used!");
        }
    }

    public bool ItemCanBeUsed(int itemInventoryIndex) // Returns if the item can be used
    {

        return playerInventory[itemInventoryIndex].itemCanBeUsed;
            
    }

    public int GetSelectedItemSOIndex()
    {
        return playerInventory[selectedItemInventoryIndex].itemSOIndex;
    }

    public ItemSO GetSelectedItemSO()
    {
        return GetItemSOByItemSOIndex(playerInventory[selectedItemInventoryIndex].itemSOIndex);
    }

    public ItemSO GetItemSOByItemSOIndex(int itemSOIndex)
    {
        return itemsListSO.allItemsSOList[itemSOIndex];
    }

    private void SetCanInteractWithInventory(bool canInteract)
    {
        canInteractWithInventory = canInteract;
    }

    private void SetSelectedItemInventoryIndex(int newItemInventoryIndex)
    {
        selectedItemInventoryIndex = newItemInventoryIndex;

        player.PlayerDragController.SetDragRb(GetSelectedItemSO().rb);

        OnItemSelected?.Invoke(selectedItemInventoryIndex);

        Debug.Log($"Selected item inventory index: {selectedItemInventoryIndex}");
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInventory.OnListChanged -= PlayerInventory_OnListChanged;


            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;

            player.PlayerLauncher.OnItemLaunched -= PlayerLauncher_OnItemLaunched;
        }
    }
}
