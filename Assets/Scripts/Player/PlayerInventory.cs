using NUnit.Framework;
using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public static event Action<ItemDataStruct> OnItemChanged;

    [SerializeField] private ItemsListSO itemsListSO;

    //private Dictionary<int, ItemData> playerItemDataInventoryByIndex = new();

    private NetworkList<ItemDataStruct> playerInventory;

    //private NetworkList<ItemData> playerItemData;

    private ItemDataStruct selectedItemData;
    public ItemDataStruct SelectedItemData => selectedItemData;

    /// <summary>
    /// The index of the selected item in the player inventory
    /// </summary>
    private int selectedItemIndex;

    //private void Awake()
    //{
    //    SetPlayerItems();
    //}

    // FALTA SYNCAR COM O SERVER E O SERVER Q RANDOMIZA OS ITEMS E QTDS

    private void Awake()
    {
        playerInventory = new();
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            playerInventory.OnListChanged += PlayerInventory_OnListChanged;
        }
    }

    private void PlayerInventory_OnListChanged(NetworkListEvent<ItemDataStruct> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case NetworkListEvent<ItemDataStruct>.EventType.Add:
                Debug.Log("Item Added");
                OnItemChanged?.Invoke(changeEvent.Value);
                break;
            case NetworkListEvent<ItemDataStruct>.EventType.Value:
                Debug.Log("Item Value Changed");
                OnItemChanged?.Invoke(changeEvent.Value);
                break;
        }
    }

    public void RandomItemServerDebug()
    {
        if (!IsServer) return; //Only Server

        SetPlayerItems();
    }

    [Command("playerInventory-printPlayerInventory", MonoTargetType.All)]
    public void PrintPlayerInventory()
    {
        for (int i = 0; i < playerInventory.Count; i++)
        {
            Debug.Log($"Player: {NetworkManager.Singleton.LocalClientId} Item: {GetItemSOByIndex(playerInventory[i].itemSOIndex).itemName} Cooldown: {GetItemSOByIndex(playerInventory[i].itemSOIndex).cooldown} Can be used: {playerInventory[i].itemCanBeUsed}");
        }
    }


    private void SetPlayerItems() //Set the items that player have
    {
        int itemsInInventory = UnityEngine.Random.Range(1, itemsListSO.allItemsSOList.Count); //Random qtd of items for now


        for(int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(0, itemsListSO.allItemsSOList.Count);

            //playerItemDataInventoryByIndex.Add(i, new ItemData
            //{
            //    itemSOIndex = randomItemSOIndex,
            //    itemCooldownRemaining = 0,
            //    itemCanBeUsed = true,
            //});

            playerInventory.Add(new ItemDataStruct
            {
                itemSOIndex = randomItemSOIndex,
                itemCooldownRemaining = 0,
                itemCanBeUsed = true,
            });


        }
    }


    [Command("playerInventory-selectItemDataByIndex")]
    public void SelectItemDataByIndex(int itemIndex) // Select a item to use, UI will call this
    {

        if (!ItemCanBeUsed(itemIndex))
        {
            Debug.LogWarning("Item can't be used!");
            return;
        }


        selectedItemData = playerInventory[itemIndex];
        Debug.Log($"Selected Item: {GetItemSOByIndex(playerInventory[itemIndex].itemSOIndex).itemName}");

    }

    [Command("playerInventory-useItem")]
    [Rpc(SendTo.Server)]
    public void UseItemRpc() // Use the item, Server will call this when both players ready
    {
        if (ItemCanBeUsed(selectedItemIndex))
        {
            //Item Can be used
            Debug.Log("Using item!"); //TO DO: Implement item use

            playerInventory[selectedItemIndex] = new ItemDataStruct
            {
                itemSOIndex = selectedItemData.itemSOIndex,
                itemCooldownRemaining = GetItemSOByIndex(selectedItemData.itemSOIndex).cooldown,
                itemCanBeUsed = false,
            };
        }
    }

    [Command("playerInventory-itemCanBeUsed")]
    public bool ItemCanBeUsed(int itemIndex) // Returns if the item can be used
    {

        return playerInventory[itemIndex].itemCanBeUsed;
            
    }

    public void UnSelectItem()
    {
        //selectedItemData = null;
    }

    public ItemSO GetItemSOByIndex(int itemIndex)
    {
        return itemsListSO.allItemsSOList[itemIndex];
    }


}
