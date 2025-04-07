using NUnit.Framework.Interfaces;
using QFSW.QC;
using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : NetworkBehaviour
{
    /// <summary>
    /// Pass the index of the itemInventoryIndex
    /// </summary>
    public event Action<int> OnItemSelectedByUI;


    [BetterHeader("References")]
    [SerializeField] private GameObject playerInventoryUIBackground;
    //[SerializeField] private PlayerThrower player;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private NetworkObject playerItemSingleUIPrefab;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button openInventoryButton;
    [SerializeField] private GameObject openInventoryBackground;
    [SerializeField] private ItemsListSO itemsListSO;
    //[SerializeField] private PlayerInventory playerInventory;

    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    private void Awake()
    {

        jumpButton.onClick.AddListener(() =>
        {
            SelecItem(0); //Jump Index

            //player.PlayerInventory.SelectItemDataByItemInventoryIndex(0); //Jump Index
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

        //player.PlayerInventory.OnItemAdded += PlayerInventory_OnItemAdded;
        //player.PlayerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
        //player.PlayerInventory.OnItemSelected += PlayerInventory_OnItemSelected;

        //player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {
        if(!IsOwner) return;

        if (state == PlayerState.DraggingItem || state == PlayerState.DraggingJump)
        {
            HideInventory();
            HideInventoryButton();
        } else
        {
            ShowInventoryButton();
        }
    }

    public void HandleOnPlayerInventoryItemSelected(int itemInventoryIndex)
    {
        if (!IsOwner) return;

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

        //UpdateOpenInventoryButton();
    }

    public void HandleOnPlayerInventoryItemChanged(ItemInventoryData itemData)
    {
        if (!IsOwner) return;

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

    public void HandleOnPlayerInventoryItemAdded(ItemInventoryData itemData)
    {
        if (!IsOwner)
        {
            Debug.Log($"PlayerInventoryUI: HandleOnPlayerInventoryItemAdded - Not Owner - The owner is: {OwnerClientId} - and I am: {NetworkManager.LocalClientId}");
            return;
        }

        //Add item on list
        SpawnPlayerItemSingleUIServerRpc(itemData);

        //UpdateOpenInventoryButton();
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerItemSingleUIServerRpc(ItemInventoryData itemData)
    {
        NetworkObject playerItemSingle = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder);
        playerItemSingle.Spawn(true);

        PlayerItemSingleUI playerItemSingleUI = playerItemSingle.GetComponent<PlayerItemSingleUI>();
        playerItemSingleUI.Setup(itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName, itemsListSO.allItemsSOList[itemData.itemSOIndex].itemIcon, itemData.itemCooldownRemaining.ToString(), itemData.itemCanBeUsed, itemData.itemInventoryIndex, this);
        playerItemSingleUIs.Add(playerItemSingleUI);

        Debug.Log($"Spawned Item in network: {itemsListSO.allItemsSOList[itemData.itemSOIndex].itemName} - Index: {itemData.itemInventoryIndex} | Item SO Index: {itemData.itemSOIndex} | Item Can Be Used: {itemData.itemCanBeUsed} | Item Cooldown Remaining: {itemData.itemCooldownRemaining}");
    }


    public void SelecItem(int itemInventoryIndex)
    {
        OnItemSelectedByUI?.Invoke(itemInventoryIndex); //Notify the player that an item was selected by UI

        //UpdateOpenInventoryButton();
        HideInventory(); //hide only when selecting an item
    }

    public void UpdateOpenInventoryButton(Sprite itemIcon)
    {
        if(!IsOwner) return;

        openInventoryButton.image.sprite = itemIcon; //Show Icon of selected item
    }

    private void ToggleInventory()
    {
        if (playerInventoryUIBackground.activeSelf)
        {
            HideInventory();
        }
        else
        {
            ShowInventory();
        }
    }

    private void HideInventory()
    {
        playerInventoryUIBackground.SetActive(false);
    }

    private void ShowInventory()
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

    public override void OnGainedOwnership()
    {
        Debug.Log($"PlayerInventoryUI Gained Ownership, new owner is: {OwnerClientId}");
        //playerInventory.ResyncReconnect();
        ShowInventory();
        openInventoryBackground.SetActive(true);
    }

    //DEBUG
    [Command("checkImOwnerInventoryUI", MonoTargetType.All)]
    private void CheckImOwnerInventoryUI()
    {
        Debug.Log($"Server is the Owner of InventoryUI? {IsOwnedByServer} - OwnerId: {OwnerClientId}");

        if (!IsOwner)
        {
            return;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y + 5f, transform.position.z);
    }

}
