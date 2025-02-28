using NUnit.Framework;
using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemsListSO itemsListSO;

    private Dictionary<int, ItemData> playerItemDataByIndex = new();

    private ItemData selectedItemData;
    public ItemData SelectedItemData => selectedItemData;

    private void Awake()
    {
        SetPlayerItems();
    }

    // FALTA SYNCAR COM O SERVER E O SERVER Q RANDOMIZA OS ITEMS E QTDS

    private void SetPlayerItems() //Set the items that player have
    {
        for (int i = 0; i < itemsListSO.allItemsList.Count; i++)
        {
            playerItemDataByIndex[i] = new ItemData
            {
                itemSO = itemsListSO.allItemsList[i],
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
            selectedItemData = itemData;
            Debug.Log($"Selected Item: {selectedItemData}");
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
}
