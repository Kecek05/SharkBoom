using NUnit.Framework.Interfaces;
using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : NetworkBehaviour
{
    
    public event Action<int> OnItemSelected;

    [BetterHeader("References")]
    [SerializeField] private Player player;
    [SerializeField] private GameObject playerInventoryUIBackground;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;
    [SerializeField] private Button useItemButton;

    [SerializeField] private ItemsListSO itemsListSO; //TEMP

    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    private void Awake()
    {
        useItemButton.onClick.AddListener(() =>
        {
            //playerInventory.UseItemRpc();
            player.SetPlayerReady();
            Debug.Log("Use Item Button Clicked");
        });
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Hide();
            return;
        }

        //Owner Code below

        player.OnPlayerReady += Player_OnPlayerReady;
        GameFlowManager.OnRoundPreparing += GameFlowManager_OnRoundPreparing;
        playerInventory.OnItemAdded += PlayerInventory_OnItemAdded;
        playerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
    }




    private void PlayerInventory_OnItemChanged(ItemDataStruct itemData)
    {
        //Update item on list
        foreach (PlayerItemSingleUI playerItemSingleUI in playerItemSingleUIs)
        {
            if (playerItemSingleUI.ItemIndex == itemData.itemInventoryIndex)
            {
                playerItemSingleUI.UpdateCooldown(itemData.itemCooldownRemaining.ToString());
                playerItemSingleUI.UpdateCanBeUsed(itemData.itemCanBeUsed);
                Debug.Log($"Item Updated UI SingleUIIndex: {playerItemSingleUI.ItemIndex} ItemDataInventoryIndex: {itemData.itemInventoryIndex} ");
                return;
            }
        }
    }

    private void PlayerInventory_OnItemAdded(ItemDataStruct itemData)
    {
        //Add item on list
        PlayerItemSingleUI playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder).GetComponent<PlayerItemSingleUI>();
        playerItemSingleUI.Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.ownerDebug, itemData.itemCanBeUsed, itemData.itemInventoryIndex);
        playerItemSingleUI.OnItemSingleSelected += (int index) => OnItemSelected?.Invoke(index);
        playerItemSingleUIs.Add(playerItemSingleUI);
        Debug.Log("Item Added UI");
    }

    private void Player_OnPlayerReady()
    {
        Hide();
    }

    private void GameFlowManager_OnRoundPreparing()
    {
        Show();
    }



    private void Hide()
    {
        playerInventoryUIBackground.SetActive(false);
    }

    private void Show()
    {
        playerInventoryUIBackground.SetActive(true);
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        foreach (PlayerItemSingleUI playerItemSingleUI in playerItemSingleUIs)
        {
            playerItemSingleUI.OnItemSingleSelected -= (int index) => OnItemSelected?.Invoke(index);
        }

        player.OnPlayerReady -= Player_OnPlayerReady;
        GameFlowManager.OnRoundPreparing -= GameFlowManager_OnRoundPreparing;
        playerInventory.OnItemAdded -= PlayerInventory_OnItemAdded;
        playerInventory.OnItemChanged -= PlayerInventory_OnItemChanged;
    }

}
