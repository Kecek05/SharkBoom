using NUnit.Framework;
using QFSW.QC;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] private ItemsListSO itemsListSO;

    private Dictionary<int, ItemData> playerItemDataInventoryByIndex = new();


    //private NetworkList<ItemData> playerItemData;

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

    [Command("playerInventory-printPlayerInventory", MonoTargetType.All)]
    public void PrintPlayerInventory()
    {
        for (int i = 0; i < playerItemDataInventoryByIndex.Count; i++)
        {
            Debug.Log($"Player: {OwnerClientId} Item: {GetItemSOByIndex(playerItemDataInventoryByIndex[i].itemSOIndex).itemName} Qtd: {playerItemDataInventoryByIndex[i].itemUsesLeft}");
        }
    }


    private void SetPlayerItems() //Set the items that player have
    {
        int itemsInInventory = Random.Range(1, itemsListSO.allItemsSOList.Count); //Random qtd of items for now


        for(int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = Random.Range(0, itemsListSO.allItemsSOList.Count);

            playerItemDataInventoryByIndex.Add(randomItemSOIndex, new ItemData
            {
                itemSOIndex = randomItemSOIndex,
                itemUsesLeft = Random.Range(1, 4), //Random qtd of the item for now
                itemCanBeUsed = true,
            });

            Debug.Log($"Player: {gameObject.name} Creating Items... Item: {GetItemSOByIndex(playerItemDataInventoryByIndex[i].itemSOIndex).itemName} Qtd: {playerItemDataInventoryByIndex[i].itemUsesLeft}");
        }
    }


    [Command("playerInventory-selectItemDataByIndex")]
    public void SelectItemDataByIndex(int itemIndex) // Select a item to use, UI will call this
    {
        if (playerItemDataInventoryByIndex.TryGetValue(itemIndex, out ItemData itemData)) 
        {
            if (!itemData.itemCanBeUsed || !StillHaveItemCount(itemIndex))
            {
                Debug.LogWarning("Item can't be used!");
                return;
            }

            selectedItemData = itemData;
            Debug.Log($"Selected Item: {GetItemSOByIndex(playerItemDataInventoryByIndex[itemIndex].itemSOIndex).itemName}");
        }
    }

    [Command("playerInventory-useItemByIndex")]
    public void UseItemByIndex(int itemIndex, int usedCount = 1) // Use the item, Player wil call this
    {
        if(playerItemDataInventoryByIndex.TryGetValue(itemIndex, out ItemData itemData))
        {
            itemData.itemUsesLeft -= usedCount;
            Debug.Log($"New item count: {playerItemDataInventoryByIndex[itemIndex].itemUsesLeft}");
        }
    }

    [Command("playerInventory-stillHaveItemCount")]
    public bool StillHaveItemCount(int itemIndex) // Returns if the item can be used
    {
        if(playerItemDataInventoryByIndex.TryGetValue(itemIndex, out ItemData itemData))
        {
            //Index found
            return playerItemDataInventoryByIndex[itemIndex].itemUsesLeft > 0;
        }

        Debug.LogWarning("Item Index not found!");
        return false; //Index not found
            
    }

    public void UnSelectItem()
    {
        selectedItemData = null;
    }

    public ItemSO GetItemSOByIndex(int itemIndex)
    {
        return itemsListSO.allItemsSOList[itemIndex];
    }


}
