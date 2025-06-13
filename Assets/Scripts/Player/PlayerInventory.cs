using System;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class PlayerInventory : NetworkBehaviour
{
    private const int JUMP_ITEM_ID = 0;
    
    public event Action<ItemInventoryData> OnItemAdded;
    public event Action<ItemInventoryData> OnItemChanged;

    /// <summary>
    /// Send the index of the selected item in the player's inventory
    /// </summary>
    public event Action<int> OnItemSelected;

    public event Action<ItemSO> OnItemSelectedSO;

    [SerializeField] private ItemsListSO itemsListSO;
    
    private NetworkList<ItemInventoryData> playerItemsInventory = new();

   //private List<ItemInventoryData> playerItemsInventory = new();
    /// <summary>
    /// The index of the selected item in the player's inventory
    /// </summary>
    //private int selectedItemInventoryIndex = 0;

    /// <summary>
    /// The ID of the selected item in the player's inventory
    /// </summary>
    private int selectedItemID = 0;

    //public int SelectedItemInventoryIndex => selectedItemInventoryIndex;


    private bool canInteractWithInventory = false;

    public bool CanInteractWithInventory => canInteractWithInventory; //DEBUG
    
    public int SelectedItemID => selectedItemID; //DEBUG

    public void Initialize()
    {
        /*SetCanInteractWithInventory(true);

        OnItemSelected?.Invoke(selectedItemInventoryIndex);
        playerItemsInventory.OnListChanged += PlayerInventory_OnListChanged; //Local event*/
        
    }
    
    public void InitializeOwner()
    {
        if(!IsOwner) return;
        
        SetCanInteractWithInventory(true);

        //OnItemSelected?.Invoke(selectedItemInventoryIndex);
        OnItemSelected?.Invoke(selectedItemID);
        
        playerItemsInventory.OnListChanged += PlayerInventory_OnListChanged; //Local event

        /*SetCanInteractWithInventory(true);*/
    }

    public void HandleOnPlayerLauncherItemLaunched(int itemInventoryIndex)
    {
        //if(!IsOwner) return;
        //item Launched
        if (itemInventoryIndex == JUMP_ITEM_ID) //Jumped
        {
            SetSelectedItemInventoryID(SelectFirstItemInventoryIndexAvailable(1)); // direct on Set to ignore canInteractWithInventory
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
                if (!ItemCanBeUsed(selectedItemID)) // If selected item can't be used, select another one
                    SelectItemDataByItemInventoryID(SelectFirstItemInventoryIndexAvailable());
                break;
            case PlayerState.IdleMyTurn:
            case PlayerState.IdleEnemyTurn:
                SetCanInteractWithInventory(true);
                break;
            case PlayerState.DraggingJump:
            case PlayerState.DraggingItem:
                SetCanInteractWithInventory(false);
                if (state == PlayerState.DragReleaseJump)
                {
                    // Jumped, can shoot
                    ChangeJumpItemServerRpc(false);
                }
                break;
            case PlayerState.DragReleaseItem:
            case PlayerState.DragReleaseJump:
                SetCanInteractWithInventory(false);
                UseAnyItemByIDServerRpc(selectedItemID); //item released, use item
                if (state == PlayerState.DragReleaseJump)
                {
                    // Jumped, can shoot
                    ChangeJumpItemServerRpc(false);
                }
                break;
            case PlayerState.MyTurnEnded:
                SetCanInteractWithInventory(false);
                DecreaseItemsCooldownServerRpc();
                ChangeJumpItemServerRpc(true); // Can jump, set before next round to be able to select
                break;
            case PlayerState.PlayerGameOver:
                SetCanInteractWithInventory(false);
                break;
        }
    }

    /*private void SetPlayerCanJump(bool canJump)
    {
        ChangeJumpItem(canJump);
        SetPlayerCanJumpServerRpc(canJump);
    }
    
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void SetPlayerCanJumpServerRpc(bool canJump)
    {
        ChangeJumpItem(canJump);
    }*/
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void ChangeJumpItemServerRpc(bool canJump)
    {
        playerItemsInventory[JUMP_ITEM_ID] = new ItemInventoryData
        {
            itemID = JUMP_ITEM_ID,
            itemCooldownRemaining = GetItemSOByItemID(JUMP_ITEM_ID).cooldown, // No cooldown
            itemCanBeUsed = canJump, // if jumped, cant jump
        };
        OnItemChanged?.Invoke(playerItemsInventory[JUMP_ITEM_ID]);
    }

    /*private void TriggerDecreaseItemsCooldown()
    {
        DecreaseItemsCooldown();
        DecreaseAllItemsCooldownServerRpc();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void DecreaseAllItemsCooldownServerRpc()
    {
        DecreaseItemsCooldown();
    }*/

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void DecreaseItemsCooldownServerRpc()
    {
        for (int i = 0; i < playerItemsInventory.Count; i++)
        {
            if (!playerItemsInventory[i].itemCanBeUsed)
            {
                playerItemsInventory[i] = new ItemInventoryData
                {
                    itemID = playerItemsInventory[i].itemID,
                    itemCooldownRemaining = playerItemsInventory[i].itemCooldownRemaining - 1,
                    itemCanBeUsed = playerItemsInventory[i].itemCooldownRemaining - 1 <= 0, // if less or equal than 0, can be used
                };
                OnItemChanged?.Invoke(playerItemsInventory[i]);
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
        Debug.LogWarning("No item available to use");
        return -1;
    }

    private void PlayerInventory_OnListChanged(NetworkListEvent<ItemInventoryData> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<ItemInventoryData>.EventType.Add:
                if (changeEvent.Value.itemID == 0) return; //Dont add item UI on Jump, ID 0 is jump
                OnItemAdded?.Invoke(changeEvent.Value);
                break;
            case NetworkListEvent<ItemInventoryData>.EventType.Value:
                OnItemChanged?.Invoke(changeEvent.Value);
                break;
        }
    }

    /// <summary>
    /// Add the items that player have when starting the game
    /// </summary>
    /// <param name="itemSOIndex"></param>
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void AddPlayerItemsServerRpc(int newItemID, int startCooldown = 0)
    {
        playerItemsInventory.Add(new ItemInventoryData
        {
            itemID = newItemID, //get the index
            itemCooldownRemaining = startCooldown,
            itemCanBeUsed = true,
        });
        
        //OnItemAdded?.Invoke(playerItemsInventory.Find(item => item.itemID == newItemID));
        Debug.Log($"PlayerInventory - Item Added Ite SO ID {newItemID} - Player Items Inventory Index: {playerItemsInventory.Count}");
        
        /*AddItemIDToList(newItemID);
        AddPlayerItemsServerRpc(newItemID);*/
    }
    
    /*[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void AddPlayerItemsServerRpc(int newItemID)
    {
        AddItemIDToList(newItemID);
        AddPlayerItemsClientRpc(newItemID);
    }
    
    [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Reliable)]
    private void AddPlayerItemsClientRpc(int newItemID)
    {
        AddItemIDToList(newItemID);
    }*/

    /*private void AddItemIDToList(int newItemID)
    {
        playerItemsInventory.Add(new ItemInventoryData
        {
            itemID = newItemID, //get the index
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });
        
        //OnItemAdded?.Invoke(playerItemsInventory.Find(item => item.itemID == newItemID));
        Debug.Log($"PlayerInventory - Item Added Ite SO ID {newItemID} - Player Items Inventory Index: {playerItemsInventory.Count}");
        
    }*/

    public void SelectItemDataByItemInventoryID(int itemInventoryID = 0) // Select a item to use, UI will call this, default (0) its Jump
    {
        Debug.Log($"SelectItemDataByItemInventoryID - {itemInventoryID} - CanInteractWithInventory: {canInteractWithInventory}");
        if (!canInteractWithInventory) return;


        if (!ItemCanBeUsed(itemInventoryID))
        {
            Debug.LogWarning("Item can't be selected!");
            return;
        }

        SetSelectedItemInventoryID(itemInventoryID);

    }

    /*private void TriggerUseItemID(int itemInventoryID)
    {
        if (ItemCanBeUsed(itemInventoryID))
        {
            //Item Can be used, use it!
            UseAnyItemByID(itemInventoryID);
            UseItemByIDServerRpc(itemInventoryID);

        } else
        {
            Debug.LogWarning("Used an item that can't be used!");
        }
    }
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void UseItemByIDServerRpc(int itemInventoryID) // Use the item, Server will call this when both players ready
    {
        UseAnyItemByID(itemInventoryID);
    }*/

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void UseAnyItemByIDServerRpc(int itemIdToUse)
    {
        for (int i = 0; i < playerItemsInventory.Count; i++)
        {
            if (playerItemsInventory[i].itemID == itemIdToUse)
            {
                playerItemsInventory[i] = new ItemInventoryData
                {
                    itemID = itemIdToUse, //do not lose the index
                    itemCooldownRemaining = GetItemSOByItemID(itemIdToUse).cooldown,
                    itemCanBeUsed = false,
                };
                OnItemChanged?.Invoke(playerItemsInventory[i]);
                return;
            }
        }
        Debug.LogWarning($"Not Found Any Item with ID: {itemIdToUse} in the Inventory to use it!");
        return;
        
        /*
        int index = playerItemsInventory.FindIndex(item => item.itemID == itemInventoryID);
        if (index == -1)
        {
            Debug.LogWarning("Not Found Any Item with this ID in the Inventory to use it!");
            return;
        }
            
        playerItemsInventory[index] = new ItemInventoryData
        {
            itemID = itemInventoryID, //do not lose the index
            itemCooldownRemaining = GetItemSOByItemID(itemInventoryID).cooldown,
            itemCanBeUsed = false,
        };
        OnItemChanged?.Invoke(playerItemsInventory[index]);*/
    }

    public bool ItemCanBeUsed(int itemID) // Returns if the item can be used
    {
        foreach (ItemInventoryData checkItemInventoryData in playerItemsInventory)
        {
            if (checkItemInventoryData.itemID == itemID)
            {
                Debug.Log($"Checking Item ID: {itemID} - Can be used: {checkItemInventoryData.itemCanBeUsed}");
                return checkItemInventoryData.itemCanBeUsed;
            }
        }
        Debug.LogWarning($"Not Found Item with ID: {itemID} in the Player Inventory to check if can be used");
        return false;
        
        //return playerItemsInventory[itemInventoryIndex].itemCanBeUsed;
    }

    /*public int GetSelectedItemID()
    {
        return playerItemsInventory[selectedItemInventoryIndex].itemSOIndex;
    }*/

    public ItemSO GetSelectedItemSO()
    {
        //return GetItemSOByItemID(playerItemsInventory[selectedItemInventoryIndex].itemSOIndex);
        return GetItemSOByItemID(selectedItemID);
    }
    


    public ItemSO GetItemSOByItemID(int itemID)
    {
        foreach (ItemInventoryData checkItemInventoryData in playerItemsInventory)
        {
            if (checkItemInventoryData.itemID == itemID)
            {
                selectedItemID = itemID;
                return itemsListSO.allItemsSOList.FirstOrDefault(itemSO => itemSO.itemID == itemID);
            }
        }
        Debug.LogWarning($"Didnt found any ItemSO with ID: {itemID} in the Player Inventory");
        return null;

        /*ItemInventoryData foundItemInventoryData = playerItemsInventory.FirstOrDefault(item => item.itemID == itemID);


        return itemsListSO.allItemsSOList[playerItemsInventory.IndexOf(foundItemInventoryData)];

        for (int i = 0; i < playerItemsInventory.Count; i++)
        {
            if (itemID == playerItemsInventory[i].itemID)
            {
                return itemsListSO.allItemsSOList[i];
            }
        }*/
    }

    private void SetCanInteractWithInventory(bool canInteract)
    {
        canInteractWithInventory = canInteract;
    }

    private void SetSelectedItemInventoryID(int newItemInventoryID)
    {
        SetItem(newItemInventoryID);
        
        PassItemIndexToServerRpc(newItemInventoryID);
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void PassItemIndexToServerRpc(int newItemInventoryID)
    {
        PassItemIndexToClientRpc(newItemInventoryID);
    }

    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void PassItemIndexToClientRpc(int newItemInventoryID)
    {
        SetItem(newItemInventoryID);
    }

    private void SetItem(int newItemInventoryID)
    {
        selectedItemID = newItemInventoryID;

        OnItemSelected?.Invoke(selectedItemID);

        OnItemSelectedSO?.Invoke(GetItemSOByItemID(selectedItemID));
        
        Debug.Log($"SetItem - Selected Item ID: {selectedItemID} - CanInteractWithInventory: {canInteractWithInventory}");
    }
    
    public void HandleOnGainOwnership()
    {
        for (int i = 1; i < playerItemsInventory.Count; i++)
        {
            //Need to be a for to start from index 1, index 0 is Jump
            OnItemAdded?.Invoke(playerItemsInventory[i]);
        }
        Debug.Log($"HandleOnGainOwnership - Items: {playerItemsInventory.Count}");
        SelectItemDataByItemInventoryID(SelectFirstItemInventoryIndexAvailable());
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            /*playerItemsInventory.OnListChanged -= PlayerInventory_OnListChanged;*/
        }
        //playerItemsInventory.OnListChanged -= PlayerInventory_OnListChanged;
    }
}
