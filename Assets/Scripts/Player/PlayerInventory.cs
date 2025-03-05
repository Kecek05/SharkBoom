using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<ItemDataStruct> OnItemAdded;
    public event Action<ItemDataStruct> OnItemChanged;


    [SerializeField] private ItemsListSO itemsListSO;


    private NetworkList<ItemDataStruct> playerInventory = new();


    private NetworkVariable<ItemDataStruct> selectedItemData = new();
    public NetworkVariable<ItemDataStruct> SelectedItemData => selectedItemData;

    /// <summary>
    /// The index of the selected item in the player inventory
    /// </summary>
    private int selectedItemIndex;

    private void Awake()
    {
        //playerInventory = new();
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            playerInventory.OnListChanged += PlayerInventory_OnListChanged;

            GameFlowManager.OnRoundEnd += GameFlowManager_OnRoundEnd;
        }
    }

    private void GameFlowManager_OnRoundEnd()
    {
        UseItemRpc();
    }

    private void PlayerInventory_OnListChanged(NetworkListEvent<ItemDataStruct> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<ItemDataStruct>.EventType.Add:
                Debug.Log("Item Added");

                OnItemAdded?.Invoke(changeEvent.Value);
                break;
            case NetworkListEvent<ItemDataStruct>.EventType.Value:
                Debug.Log("Item Value Changed");
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
            Debug.Log($"Player: {gameObject.name} Item: {GetItemSOByIndex(playerInventory[i].itemSOIndex).itemName} Cooldown: {GetItemSOByIndex(playerInventory[i].itemSOIndex).cooldown} Can be used: {playerInventory[i].itemCanBeUsed} Item Inventory Index: {playerInventory[i].itemInventoryIndex}");
        }
    }
    [Command("playerInventory-printPlayerSelectedItem", MonoTargetType.All)]
    public void PrintPlayerSelectedItem() //DEBUG
    {
        Debug.Log($"Player: {gameObject.name} Selected Item: {GetItemSOByIndex(selectedItemData.Value.itemSOIndex).itemName}");
    }
    #endregion

    public void SetPlayerItems(int itemSOIndex) //Set the items that player have
    {

        playerInventory.Add(new ItemDataStruct
        {
            ownerDebug = $"Player {gameObject.name}",
            itemInventoryIndex = playerInventory.Count, //get the index
            itemSOIndex = itemSOIndex,
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });

        Debug.Log("Item Setted");
    }


    [Command("playerInventory-selectItemDataByIndex")]
    [Rpc(SendTo.Server)]
    public void SelectItemDataByIndexRpc(int itemInventoryIndex) // Select a item to use, UI will call this
    {

        if (!ItemCanBeUsed(itemInventoryIndex))
        {
            Debug.LogWarning("Item can't be used!");
            return;
        }

        selectedItemIndex = itemInventoryIndex;
        selectedItemData.Value = playerInventory[itemInventoryIndex];
        Debug.Log($"Player: {gameObject.name} Selected Item: {GetItemSOByIndex(playerInventory[itemInventoryIndex].itemSOIndex).itemName}");

    }

    [Command("playerInventory-useItem")]
    [Rpc(SendTo.Server)]
    public void UseItemRpc() // Use the item, Server will call this when both players ready
    {
        if (ItemCanBeUsed(selectedItemIndex))
        {
            //Item Can be used
            Debug.Log($"Player: {gameObject.name} Using item!"); //TO DO: Implement item use

            playerInventory[selectedItemIndex] = new ItemDataStruct
            {
                itemInventoryIndex = selectedItemData.Value.itemInventoryIndex, //do not lose the index
                itemSOIndex = selectedItemData.Value.itemSOIndex,
                itemCooldownRemaining = GetItemSOByIndex(selectedItemData.Value.itemSOIndex).cooldown,
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
        return GetItemSOByIndex(selectedItemData.Value.itemSOIndex);
    }

    public ItemSO GetItemSOByIndex(int itemSOIndex)
    {
        return itemsListSO.allItemsSOList[itemSOIndex];
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInventory.OnListChanged -= PlayerInventory_OnListChanged;
        }
    }
}
