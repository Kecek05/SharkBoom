using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<ItemInventoryData> OnItemAdded;
    public event Action<ItemInventoryData> OnItemChanged;

    /// <summary>
    /// Send the index of the selected item in the player's inventory
    /// </summary>
    public event Action<int> OnItemSelected;

    public event Action<ItemSO> OnItemSelectedSO;

    [SerializeField] private ItemsListSO itemsListSO;

    private NetworkList<ItemInventoryData> playerItemsInventory = new();

    /// <summary>
    /// The index of the selected item in the player's inventory
    /// </summary>
    private int selectedItemInventoryIndex = 0;

    public int SelectedItemInventoryIndex => selectedItemInventoryIndex;


    private bool canInteractWithInventory = false;

    public bool CanInteractWithInventory => canInteractWithInventory; //DEBUG

    [Command("printInventory", MonoTargetType.All)]
    private void PrintItemsInventory()
    {
        if (!IsOwner) return;

        foreach(ItemInventoryData item in playerItemsInventory)
        {
            Debug.Log($"Item {GetItemSOByItemSOIndex(item.itemSOIndex).itemName} - Index: {item.itemInventoryIndex} | Item SO Index: {item.itemSOIndex} | Item Can Be Used: {item.itemCanBeUsed} | Item Cooldown Remaining: {item.itemCooldownRemaining}");
        }
    }

    //public override void OnNetworkSpawn()
    //{
    //    if(IsOwner)
    //    {
    //        if(playerItemsInventory.Count > 0)
    //        {
    //            ResyncReconnect();
    //            Debug.Log("ResyncReconnect called in OnNetworkSpawn");
    //        }
    //    }
    //}

    public void InitializeOwner()
    {
        if(!IsOwner) return;

        SetCanInteractWithInventory(true);

        OnItemSelected?.Invoke(selectedItemInventoryIndex);

        playerItemsInventory.OnListChanged += PlayerInventory_OnListChanged; //Local event

    }

    public void HandleOnPlayerLauncherItemLaunched(int itemInventoryIndex)
    {
        if(!IsOwner) return;
        //item Launched
        if (itemInventoryIndex == 0) //Jumped
        {
            SetSelectedItemInventoryIndex(SelectFirstItemInventoryIndexAvailable(1)); // direct on Set to ignore canInteractWithInventory
        }
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {

        if (!IsOwner)
        {
            Debug.Log($"HandleOnPlayerStateMachineStateChanged in PlayerInventory called and not the owner - Owner Is: {OwnerClientId} - Im: {NetworkManager.LocalClientId} - Object is: {gameObject.transform.name}");
            return;
        }

        switch (state)
        {
            case PlayerState.MyTurnStarted:
                SetCanInteractWithInventory(true);
                if (!ItemCanBeUsed(selectedItemInventoryIndex)) // If selected item can't be used, select another one
                    SelectItemDataByItemInventoryIndex(SelectFirstItemInventoryIndexAvailable());
                break;
            case PlayerState.IdleMyTurn:
            case PlayerState.IdleEnemyTurn:
                SetCanInteractWithInventory(true);
                break;
            case PlayerState.DraggingJump:
            case PlayerState.DraggingItem:
            case PlayerState.DragReleaseItem:
            case PlayerState.DragReleaseJump:
                SetCanInteractWithInventory(false);
                if (state == PlayerState.DragReleaseJump)
                {
                    // Jumped, can shoot
                    SetPlayerCanJumpRpc(false);
                }
                break;
            case PlayerState.MyTurnEnded:
                SetCanInteractWithInventory(false);
                DecreaseAllItemsCooldownRpc();
                UseItemByInventoryIndexRpc(selectedItemInventoryIndex);
                SetPlayerCanJumpRpc(true); // Can jump, set before next round to be able to select
                break;
            case PlayerState.PlayerGameOver:
                SetCanInteractWithInventory(false);
                break;
        }

        Debug.Log($"HandleOnPlayerStateMachineStateChanged in PlayerInventory called and its owner");
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerCanJumpRpc(bool canJump)
    {
        playerItemsInventory[0] = new ItemInventoryData
        {
            itemInventoryIndex = playerItemsInventory[0].itemInventoryIndex, //first in inventory
            itemSOIndex = playerItemsInventory[0].itemSOIndex, //first in itemsListSO
            itemCooldownRemaining = GetItemSOByItemSOIndex(playerItemsInventory[0].itemSOIndex).cooldown, // No cooldown
            itemCanBeUsed = canJump, // if jumped, cant jump
        };
    }

    [Rpc(SendTo.Server)]
    public void DecreaseAllItemsCooldownRpc()
    {
        for (int i = 0; i < playerItemsInventory.Count; i++)
        {
            if (!playerItemsInventory[i].itemCanBeUsed)
            {
                playerItemsInventory[i] = new ItemInventoryData
                {
                    itemInventoryIndex = playerItemsInventory[i].itemInventoryIndex,
                    itemSOIndex = playerItemsInventory[i].itemSOIndex,
                    itemCooldownRemaining = playerItemsInventory[i].itemCooldownRemaining - 1,
                    itemCanBeUsed = playerItemsInventory[i].itemCooldownRemaining - 1 <= 0, // if less or equal than 0, can be used
                };
            }
        }
    }

    public int SelectFirstItemInventoryIndexAvailable(int startingIndex = 0) //can pass a index to ignore previously itens
    {
        for (int i = startingIndex; i < playerItemsInventory.Count; i++)
        {
            if (playerItemsInventory[i].itemCanBeUsed)
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
        playerItemsInventory.Add(new ItemInventoryData
        {
            itemInventoryIndex = playerItemsInventory.Count, //get the index
            itemSOIndex = itemSOIndex,
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });
    }

    public void SelectItemDataByItemInventoryIndex(int itemInventoryIndex = 0) // Select a item to use, UI will call this, default (0) its Jump
    {
        Debug.Log($"SelectItemDataByItemInventoryIndex called - Item Inventory Index Selected: {itemInventoryIndex} - can interact with inventory? {canInteractWithInventory}");

        if (!canInteractWithInventory) return;


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

            playerItemsInventory[itemInventoryIndex] = new ItemInventoryData
            {
                itemInventoryIndex = playerItemsInventory[itemInventoryIndex].itemInventoryIndex, //do not lose the index
                itemSOIndex = playerItemsInventory[itemInventoryIndex].itemSOIndex,
                itemCooldownRemaining = GetItemSOByItemSOIndex(playerItemsInventory[itemInventoryIndex].itemSOIndex).cooldown,
                itemCanBeUsed = false,
            };

        } else
        {
            Debug.LogWarning("Used an item that can't be used!");
        }
    }

    public bool ItemCanBeUsed(int itemInventoryIndex) // Returns if the item can be used
    {

        return playerItemsInventory[itemInventoryIndex].itemCanBeUsed;
            
    }

    public int GetSelectedItemSOIndex()
    {
        return playerItemsInventory[selectedItemInventoryIndex].itemSOIndex;
    }

    public ItemSO GetSelectedItemSO()
    {
        return GetItemSOByItemSOIndex(playerItemsInventory[selectedItemInventoryIndex].itemSOIndex);
    }

    public ItemSO GetItemSOByItemSOIndex(int itemSOIndex)
    {
        return itemsListSO.allItemsSOList[itemSOIndex];
    }

    private void SetCanInteractWithInventory(bool canInteract)
    {
        canInteractWithInventory = canInteract;
        Debug.Log($"SetCanInteractWithInventory - {canInteractWithInventory}");
    }

    private void SetSelectedItemInventoryIndex(int newItemInventoryIndex)
    {
        selectedItemInventoryIndex = newItemInventoryIndex;

        OnItemSelected?.Invoke(selectedItemInventoryIndex);

        OnItemSelectedSO?.Invoke(GetItemSOByItemSOIndex(playerItemsInventory[selectedItemInventoryIndex].itemSOIndex));
        Debug.Log($"Selected item inventory index: {selectedItemInventoryIndex}");
    }

    //public override void OnGainedOwnership()
    //{
    //    //for(int i = 1; i < playerItemsInventory.Count; i++)
    //    //{
    //    //    //Need to be a for to start from index 1, index 0 is Jump
    //    //    OnItemAdded?.Invoke(playerItemsInventory[i]);
    //    //}

    //    //Reselect an item


    //}

    public void HandleOnGainOwnership()
    {
        for (int i = 1; i < playerItemsInventory.Count; i++)
        {
            //Need to be a for to start from index 1, index 0 is Jump
            OnItemAdded?.Invoke(playerItemsInventory[i]);
        }

        SelectItemDataByItemInventoryIndex(SelectFirstItemInventoryIndexAvailable());
        Debug.Log($"ResyncReconnect called - Items in inventory: {playerItemsInventory.Count} - OwnerId: {OwnerClientId}");
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerItemsInventory.OnListChanged -= PlayerInventory_OnListChanged;
        }
    }
}
