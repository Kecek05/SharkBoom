using NUnit.Framework;
using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemsListSO itemsListSO;

    private Dictionary<int, ItemSO> playerItemsByIndex = new();
    private Dictionary<int, int> playerItemsCountByIndex = new();

    private ItemSO selectedItemSO;
    public ItemSO SelectedItemSO => selectedItemSO;

    private void Awake()
    {
        SetPlayerItems();
    }

    // FALTA SYNCAR COM O SERVER E O SERVER Q RANDOMIZA OS ITEMS E QTDS

    private void SetPlayerItems() //Set the items that player have
    {
        for (int i = 0; i < itemsListSO.allItemsList.Count; i++)
        {
            playerItemsByIndex[i] = itemsListSO.allItemsList[i];
            playerItemsCountByIndex[i] = Random.Range(1,4); //Random qtd of the item for now
            Debug.Log($"Player: {gameObject.name} Creating Items... Item: {playerItemsByIndex[i]} Qtd: {playerItemsCountByIndex[i]}");
        }
    }


    [Command("playerInventory-selectItemByIndex")]
    public void SelectItemByIndex(int itemIndex) // Select a item to use, UI will call this
    {
        if (playerItemsByIndex.TryGetValue(itemIndex, out ItemSO itemSO)) 
        {
            selectedItemSO = itemSO;
            Debug.Log($"Selected Item: {selectedItemSO}");
        }
    }

    [Command("playerInventory-useItemByIndex")]
    public void UseItemByIndex(int itemIndex, int usedCount = 1) // Use the item, Player wil call this
    {
        if(playerItemsCountByIndex.TryGetValue(itemIndex, out int itemCount))
        {
            playerItemsCountByIndex[itemIndex] -= usedCount;
            Debug.Log($"New item count: {playerItemsCountByIndex[itemIndex]}");
        }
    }

    [Command("playerInventory-stillHaveItemCount")]
    public bool StillHaveItemCount(int itemIndex) // Returns if the item can be used
    {
        if(playerItemsCountByIndex.TryGetValue(itemIndex, out int itemCount))
        {
            //Index found
            return playerItemsCountByIndex[itemIndex] > 0;
        }

        Debug.LogWarning("Item Index not found!");
        return false; //Index not found
            
    }
}
