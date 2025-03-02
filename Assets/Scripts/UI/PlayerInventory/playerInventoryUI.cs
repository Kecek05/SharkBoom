using NUnit.Framework;
using Sortify;
using System;
using System.Collections.Generic;
using UnityEngine;

public class playerInventoryUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;

    [SerializeField] private ItemsListSO itemsListSO; //TEMP

    private List<ItemDataStruct> playerInventory = new();

    private void Awake()
    {
        PlayerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
    }

    private void PlayerInventory_OnItemChanged(ItemDataStruct itemData)
    {
        GameObject playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder);
        playerItemSingleUI.GetComponent<playerItemSingleUI>().Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.ownerDebug);

        playerInventory.Add(itemData);

        Debug.Log("Item Added UI");
    }
}
