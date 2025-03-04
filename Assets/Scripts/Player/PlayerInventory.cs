using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public event Action<ItemDataStruct> OnItemAdded;
    public event Action<ItemDataStruct> OnItemChanged;

    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private PlayerInventoryUI playerInventoryUI; //TEMP


    private NetworkList<ItemDataStruct> playerInventory;


    private ItemDataStruct selectedItemData;
    public ItemDataStruct SelectedItemData => selectedItemData;

    /// <summary>
    /// The index of the selected item in the player inventory
    /// </summary>
    private int selectedItemIndex;


    private void Awake()
    {
        playerInventory = new();
        playerInventoryUI.OnItemSelected += PlayerInventoryUI_OnItemSelected;
    }

    private void PlayerInventoryUI_OnItemSelected(int itemIndex)
    {
        SelectItemDataByIndex(itemIndex);
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            playerInventory.OnListChanged += PlayerInventory_OnListChanged;
            Debug.Log("Im the owner");
        }

        gameObject.name = "Player " + UnityEngine.Random.Range(0, 100).ToString();
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

    [Command("playerInventory-printPlayerInventory", MonoTargetType.All)]
    public void PrintPlayerInventory()
    {
        for (int i = 0; i < playerInventory.Count; i++)
        {
            Debug.Log($"Player: {gameObject.name} Item: {GetItemSOByIndex(playerInventory[i].itemSOIndex).itemName} Cooldown: {GetItemSOByIndex(playerInventory[i].itemSOIndex).cooldown} Can be used: {playerInventory[i].itemCanBeUsed} Item Inventory Index: {playerInventory[i].itemInventoryIndex}");
        }
    }



    public void SetPlayerItems(int itemSOIndex) //Set the items that player have
    {

        playerInventory.Add(new ItemDataStruct
        {
            ownerDebug = $"Player {gameObject.name}",
            itemInventoryIndex = playerInventory.Count - 1,
            itemSOIndex = itemSOIndex,
            itemCooldownRemaining = 0,
            itemCanBeUsed = true,
        });

        Debug.Log("Item Setted");
    }


    [Command("playerInventory-selectItemDataByIndex")]
    public void SelectItemDataByIndex(int itemIndex) // Select a item to use, UI will call this
    {

        if (!ItemCanBeUsed(itemIndex))
        {
            Debug.LogWarning("Item can't be used!");
            return;
        }

        selectedItemIndex = itemIndex;
        selectedItemData = playerInventory[itemIndex];
        Debug.Log($"Player: {gameObject.name} Selected Item: {GetItemSOByIndex(playerInventory[itemIndex].itemSOIndex).itemName}");

    }

    [Command("playerInventory-useItem")]
    public void UseItem() // Use the item, Server will call this when both players ready
    {
        if (ItemCanBeUsed(selectedItemIndex))
        {
            //Item Can be used
            Debug.Log($"Player: {gameObject.name} Using item!"); //TO DO: Implement item use

            playerInventory[selectedItemIndex] = new ItemDataStruct
            {
                itemSOIndex = selectedItemData.itemSOIndex,
                itemCooldownRemaining = GetItemSOByIndex(selectedItemData.itemSOIndex).cooldown,
                itemCanBeUsed = false,
            };
        } else
        {
            Debug.LogWarning("Item can't be used!");
        }
    }

    [Command("playerInventory-itemCanBeUsed")]
    public bool ItemCanBeUsed(int itemIndex) // Returns if the item can be used
    {

        return playerInventory[itemIndex].itemCanBeUsed;
            
    }

    public ItemSO GetItemSOByIndex(int itemIndex)
    {
        return itemsListSO.allItemsSOList[itemIndex];
    }

    
}
