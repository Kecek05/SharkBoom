using NUnit.Framework;
using Sortify;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{

    public event Action<int> OnItemSelected;

    [BetterHeader("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;

    [SerializeField] private ItemsListSO itemsListSO; //TEMP

    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    private void Awake()
    {
        playerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
    }

    private void PlayerInventory_OnItemChanged(ItemDataStruct itemData)
    {
        PlayerItemSingleUI playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder).GetComponent<PlayerItemSingleUI>();
        playerItemSingleUI.Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.ownerDebug, itemData.itemCanBeUsed, itemData.itemSOIndex);

        playerItemSingleUI.OnItemSingleSelected += (int index) => OnItemSelected?.Invoke(index);

        playerItemSingleUIs.Add(playerItemSingleUI);

        Debug.Log("Item Added UI");
    }

    private void OnDestroy()
    {
        foreach (PlayerItemSingleUI playerItemSingleUI in playerItemSingleUIs)
        {
            playerItemSingleUI.OnItemSingleSelected -= (int index) => OnItemSelected?.Invoke(index);
        }
    }
}
