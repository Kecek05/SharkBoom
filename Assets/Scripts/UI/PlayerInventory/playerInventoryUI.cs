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
    [SerializeField] private PlayerThrower player;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button openInventoryButton;
    [SerializeField] private GameObject openInventoryBackground;
    [SerializeField] private ItemsListSO itemsListSO;

    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    private void Awake()
    {

        jumpButton.onClick.AddListener(() =>
        {
            player.PlayerInventory.SelectItemDataByItemInventoryIndex(0); //Jump Index
        });

        openInventoryButton.onClick.AddListener(ToggleInventory);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            HideInventory();
            openInventoryBackground.SetActive(false);
            return;
        }

        //Owner Code below

        player.PlayerInventory.OnItemAdded += PlayerInventory_OnItemAdded;
        player.PlayerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
        player.PlayerInventory.OnItemSelected += PlayerInventory_OnItemSelected;

        player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

    }

    private void PlayerStateMachine_OnStateChanged(IState state)
    {
        if (state == player.PlayerStateMachine.draggingItem || state == player.PlayerStateMachine.draggingJump)
        {
            HideInventory();
            HideInventoryButton();
        } else
        {
            ShowInventoryButton();
        }
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

        UpdateOpenInventoryButton();
    }

    private void PlayerInventory_OnItemChanged(ItemInventoryData itemData)
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

    private void PlayerInventory_OnItemAdded(ItemInventoryData itemData)
    {
        //Add item on list
        PlayerItemSingleUI playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder).GetComponent<PlayerItemSingleUI>();
        playerItemSingleUI.Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.itemCanBeUsed, itemData.itemInventoryIndex, this);
        playerItemSingleUIs.Add(playerItemSingleUI);

        UpdateOpenInventoryButton();
    }

    public void SelecItem(int itemInventoryIndex)
    {
        player.PlayerInventory.SelectItemDataByItemInventoryIndex(itemInventoryIndex);

        UpdateOpenInventoryButton();
        HideInventory(); //hide only when selecting an item
    }

    private void UpdateOpenInventoryButton()
    {
        openInventoryButton.image.sprite = player.PlayerInventory.GetSelectedItemSO().itemIcon; //Show Icon of selected item
    }

    private void ToggleInventory()
    {
        if (playerInventoryUIBackground.activeSelf)
        {
            HideInventory();
        }
        else
        {
            ShoInventory();
        }
    }

    private void HideInventory()
    {
        playerInventoryUIBackground.SetActive(false);
    }

    private void ShoInventory()
    {
        playerInventoryUIBackground.SetActive(true);
    }

    private void HideInventoryButton()
    {
        openInventoryBackground.SetActive(false);
    }

    private void ShowInventoryButton()
    {
        openInventoryBackground.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;


        player.PlayerInventory.OnItemAdded -= PlayerInventory_OnItemAdded;
        player.PlayerInventory.OnItemChanged -= PlayerInventory_OnItemChanged;

        player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
    }

}
