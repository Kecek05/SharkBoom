using NUnit.Framework.Interfaces;
using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : NetworkBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private GameObject playerInventoryUIBackground;
    [SerializeField] private Player player;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;
    [SerializeField] private Button jumpButton;

    [SerializeField] private ItemsListSO itemsListSO; //TEMP

    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    private void Awake()
    {

        jumpButton.onClick.AddListener(() =>
        {
            player.PlayerInventory.SelectItemDataByItemInventoryIndex(0); //Jump Index
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
        player.PlayerInventory.OnItemAdded += PlayerInventory_OnItemAdded;
        player.PlayerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
        player.PlayerInventory.OnItemSelected += PlayerInventory_OnItemSelected;
    }

    private void PlayerInventory_OnItemSelected(int itemInventoryIndex)
    {
        //Update item on list
        foreach (PlayerItemSingleUI playerItemSingleUI in playerItemSingleUIs)
        {
            if (playerItemSingleUI.ItemIndex == itemInventoryIndex)
            {
                playerItemSingleUI.SelectedThisItem();
            } else
            {
                playerItemSingleUI.UnSelectedThisItem();
            }
        }
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
                return;
            }
        }
    }

    private void PlayerInventory_OnItemAdded(ItemDataStruct itemData)
    {
        //Add item on list
        PlayerItemSingleUI playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder).GetComponent<PlayerItemSingleUI>();
        playerItemSingleUI.Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.itemCanBeUsed, itemData.itemInventoryIndex, this);
        playerItemSingleUIs.Add(playerItemSingleUI);
    }

    public void SelecItem(int itemInventoryIndex)
    {
        player.PlayerInventory.SelectItemDataByItemInventoryIndex(itemInventoryIndex);
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

        player.OnPlayerReady -= Player_OnPlayerReady;
        GameFlowManager.OnRoundPreparing -= GameFlowManager_OnRoundPreparing;
        player.PlayerInventory.OnItemAdded -= PlayerInventory_OnItemAdded;
        player.PlayerInventory.OnItemChanged -= PlayerInventory_OnItemChanged;
    }

}
