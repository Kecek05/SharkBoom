using QFSW.QC;
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


    private NetworkVariable<ItemDataStruct> selectedItemData = new();
    public NetworkVariable<ItemDataStruct> SelectedItemData => selectedItemData;

    /// <summary>
    /// The index of the selected item in the player inventory
    /// </summary>
    private int selectedItemIndex;


    private bool canInteractWithInventory = false;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {

            player.OnPlayerJumped += Player_OnPlayerJumped;
            player.OnPlayerShooted += Player_OnPlayerShooted;

            playerInventory.OnListChanged += PlayerInventory_OnListChanged;

            player.OnPlayerCanPlay += Player_OnPlayerCanPlay;
            player.OnPlayerCantPlay += Player_OnPlayerCantPlay;

        }
    }

    private void Player_OnPlayerCanPlay()
    {
        //Can interact with inventory

        canInteractWithInventory = true;
    }


    private void Player_OnPlayerCantPlay()
    {
        //Cant interact with inventory
        canInteractWithInventory = false;
    }


    private void Player_OnPlayerJumped()
    {
        //Jumped, can shoot
        SetPlayerJumpedRpc(true);
        SelectItemDataByItemInventoryIndexRpc(SelectFirstItemInventoryIndexAvailable());
    }

    private void Player_OnPlayerShooted()
    {
        //Round ended
        DecreaseAllItemsCooldownRpc();
        UseItemByInventoryIndexRpc(selectedItemData.Value.itemInventoryIndex);
        SetPlayerJumpedRpc(false);
        SelectItemDataByItemInventoryIndexRpc();
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerJumpedRpc(bool jumped)
    {
        playerInventory[0] = new ItemDataStruct
        {
            itemInventoryIndex = playerInventory[0].itemInventoryIndex,
            itemSOIndex = playerInventory[0].itemSOIndex,
            itemCooldownRemaining = playerInventory[0].itemCooldownRemaining - 1,
            itemCanBeUsed = !jumped, // if jumped, cant jump
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
                break;
        }
    }

    #region DEBUG

    [Command("playerInventory-printPlayerInventory", MonoTargetType.All)]
    public void PrintPlayerInventory() //DEBUG
    {
        for (int i = 0; i < playerInventory.Count; i++)
        {
            Debug.Log($"Player: {gameObject.name} Item: {GetItemSOByItemSOIndex(playerInventory[i].itemSOIndex).itemName} Cooldown: {GetItemSOByItemSOIndex(playerInventory[i].itemSOIndex).cooldown} Can be used: {playerInventory[i].itemCanBeUsed} Item Inventory Index: {playerInventory[i].itemInventoryIndex}");
        }
    }
    [Command("playerInventory-printPlayerSelectedItem", MonoTargetType.All)]
    public void PrintPlayerSelectedItem() //DEBUG
    {
        Debug.Log($"Player: {gameObject.name} Selected Item: {GetItemSOByItemSOIndex(selectedItemData.Value.itemSOIndex).itemName}");
    }
    #endregion


    public void SetPlayerItems(int itemSOIndex) //Set the items that player have when starting the game
    {

        playerInventory.Add(new ItemDataStruct
        {
            itemInventoryIndex = playerInventory.Count, //get the index
            itemSOIndex = itemSOIndex,
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });
        SelectItemDataByItemInventoryIndexRpc(); //Default to Jump
    }


    [Command("playerInventory-selectItemDataByIndex")]
    [Rpc(SendTo.Server)]
    public void SelectItemDataByItemInventoryIndexRpc(int itemInventoryIndex = 0) // Select a item to use, UI will call this, default (0) its Jump
    {
        if(!canInteractWithInventory) return;


        if (!ItemCanBeUsed(itemInventoryIndex))
        {
            Debug.LogWarning("Item can't be used!");
            return;
        }

        selectedItemIndex = itemInventoryIndex;
        selectedItemData.Value = playerInventory[itemInventoryIndex];

        TriggerSetDragAndShootRpc();
        TriggerOnItemSelectedClientsRpc(itemInventoryIndex);

    }

    [Rpc(SendTo.Owner)]
    protected void TriggerSetDragAndShootRpc()
    {
        player.PlayerDragController.SetDragAndShoot(GetSelectedItemSO().rb);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnItemSelectedClientsRpc(int itemInventoryIndex)
    {
        if(!IsOwner) return;

        OnItemSelected?.Invoke(itemInventoryIndex);
    }

    [Command("playerInventory-useItem")]
    [Rpc(SendTo.Server)]
    public void UseItemByInventoryIndexRpc(int itemInventoryIndex) // Use the item, Server will call this when both players ready
    {
        if (!canInteractWithInventory);

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


    [Command("playerInventory-itemCanBeUsed")]
    public bool ItemCanBeUsed(int itemInventoryIndex) // Returns if the item can be used
    {

        return playerInventory[itemInventoryIndex].itemCanBeUsed;
            
    }

    public ItemSO GetSelectedItemSO()
    {
        return GetItemSOByItemSOIndex(selectedItemData.Value.itemSOIndex);
    }

    public ItemSO GetItemSOByItemSOIndex(int itemSOIndex)
    {
        return itemsListSO.allItemsSOList[itemSOIndex];
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInventory.OnListChanged -= PlayerInventory_OnListChanged;

            player.OnPlayerJumped -= Player_OnPlayerJumped;
            player.OnPlayerShooted -= Player_OnPlayerShooted;

            player.OnPlayerCanPlay -= Player_OnPlayerCanPlay;
            player.OnPlayerCantPlay -= Player_OnPlayerCantPlay;
        }
    }
}
