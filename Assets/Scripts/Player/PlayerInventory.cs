using NUnit.Framework;
using QFSW.QC;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] private ItemsListSO itemsListSO;

    private Dictionary<int, ItemData> playerItemDataByIndex = new();

    private ItemData selectedItemData;
    public ItemData SelectedItemData => selectedItemData;

    //private void Awake()
    //{
    //    SetPlayerItems();
    //}

    // FALTA SYNCAR COM O SERVER E O SERVER Q RANDOMIZA OS ITEMS E QTDS

    //public override void OnNetworkSpawn()
    //{
    //    RandomItemServerDebug();
    //}

    public void RandomItemServerDebug()
    {
        if (!IsServer) return; //Only Server

        SetPlayerItems();
    }

    [Command("playerInventory-printPlayerInventory")]
    public void PrintPlayerInventory()
    {
        for (int i = 0; i < playerItemDataByIndex.Count; i++)
        {
            Debug.Log($"Player: {gameObject.name} Item: {playerItemDataByIndex[i].itemSO.itemName} Qtd: {playerItemDataByIndex[i].itemUsesLeft}");
        }
    }

    private void SetPlayerItems() //Set the items that player have
    {
        for (int i = 0; i < itemsListSO.allItemsSOList.Count; i++)
        {
            playerItemDataByIndex[i] = new ItemData
            {
                itemSO = itemsListSO.allItemsSOList[i],
                itemIndex = i,
                itemUsesLeft = Random.Range(1, 4), //Random qtd of the item for now
                itemCanBeUsed = true
            };

            Debug.Log($"Player: {gameObject.name} Creating Items... Item: {playerItemDataByIndex[i].itemSO.name} Qtd: {playerItemDataByIndex[i].itemUsesLeft}");
        }
    }


    [Command("playerInventory-selectItemDataByIndex")]
    public void SelectItemDataByIndex(int itemIndex) // Select a item to use, UI will call this
    {
        if (playerItemDataByIndex.TryGetValue(itemIndex, out ItemData itemData)) 
        {
            if (!itemData.itemCanBeUsed || !StillHaveItemCount(itemIndex))
            {
                Debug.LogWarning("Item can't be used!");
                return;
            }

            selectedItemData = itemData;
            Debug.Log($"Selected Item: {selectedItemData.itemSO.itemName}");
        }
    }

    [Command("playerInventory-useItemByIndex")]
    public void UseItemByIndex(int itemIndex, int usedCount = 1) // Use the item, Player wil call this
    {
        if(playerItemDataByIndex.TryGetValue(itemIndex, out ItemData itemData))
        {
            itemData.itemUsesLeft -= usedCount;
            Debug.Log($"New item count: {playerItemDataByIndex[itemIndex].itemUsesLeft}");
        }
    }

    [Command("playerInventory-stillHaveItemCount")]
    public bool StillHaveItemCount(int itemIndex) // Returns if the item can be used
    {
        if(playerItemDataByIndex.TryGetValue(itemIndex, out ItemData itemData))
        {
            //Index found
            return playerItemDataByIndex[itemIndex].itemUsesLeft > 0;
        }

        Debug.LogWarning("Item Index not found!");
        return false; //Index not found
            
    }

    public void UnSelectItem()
    {
        selectedItemData = null;
    }
}
