using NUnit.Framework.Interfaces;
using QFSW.QC;
using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerInventoryUI : NetworkBehaviour
{
    /// <summary>
    /// Pass the index of the itemInventoryIndex
    /// </summary>
    public event Action<int> OnItemSelectedByUI;

    [BetterHeader("References")]
    [SerializeField] private GameObject playerInventoryUIBackground;

    [SerializeField] private Transform inventoryParent;
    [SerializeField] private Transform inventoryItemHolder;
    [SerializeField] private GameObject playerItemSingleUIPrefab;
    [SerializeField] private Image selectedItemImage;
    [SerializeField] private GameObject openInventoryBackground;
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private GameObject tutorialParent;
    [SerializeField] private GameObject tutorialButton;

    private Camera cameraUI;
    private List<PlayerItemSingleUI> playerItemSingleUIs = new();

    public override void OnNetworkSpawn()
    {
        HideInventoryBackground();
        HideInventoryButton();
        HideInventoryParent();
        SetupInventoryWorldCamera();
        tutorialParent.SetActive(false);
        tutorialButton.SetActive(false);
        Debug.Log($"HIDE INVENTOR - {gameObject.transform.parent.parent.name}");
    }

    public void SelectJumpButton()
    {
        SelecItem(0); //Jump ID
    }

    public void InventoryButton()
    {
        ToggleInventory();
    }
        
    private void SetupInventoryWorldCamera()
    {
        UniversalAdditionalCameraData data = ServiceLocator.Get<Camera>().GetComponent<UniversalAdditionalCameraData>();
        List<Camera> stack = data.cameraStack;

        if (stack != null)
        {
            foreach (var overlayCam in stack)
                cameraUI = overlayCam;
        }

        inventoryCanvas.worldCamera = cameraUI;
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {
        if(!IsOwner) return;

        if (state == PlayerState.DraggingItem || state == PlayerState.DraggingJump)
        {
            HideInventoryBackground();
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
            if (playerItemSingleUI.MyItemID == itemInventoryIndex)
            {
                playerItemSingleUI.SelectedThisItem();
            } else
            {
                playerItemSingleUI.UnSelectedThisItem();
            }
        }

        HideInventoryBackground();
    }

    public void HandleOnPlayerInventoryItemChanged(ItemInventoryData itemData)
    {
        if (!IsOwner) return;

        //Update item on list
        foreach (PlayerItemSingleUI playerItemSingleUI in playerItemSingleUIs)
        {
            if (playerItemSingleUI.MyItemID == itemData.itemID)
            {
                playerItemSingleUI.UpdateCooldown(itemData.itemCooldownRemaining);
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
        PlayerItemSingleUI playerItemSingleUI = Instantiate(playerItemSingleUIPrefab, inventoryItemHolder).GetComponent<PlayerItemSingleUI>();
        
        ItemSO newItemSO = itemsListSO.allItemsSOList.Find(itemSO => itemSO.itemID == itemData.itemID);
        
        playerItemSingleUI.Setup(newItemSO.itemName, newItemSO.itemIcon, itemData.itemCooldownRemaining, itemData.itemCanBeUsed, newItemSO.itemID, newItemSO.damageableSO.damage, this);
        playerItemSingleUIs.Add(playerItemSingleUI);
    }


    public void SelecItem(int itemID)
    {
        OnItemSelectedByUI?.Invoke(itemID); //Notify the player that an item was selected by UI
        // Debug.Log($"Trying to select item with ID: {itemID}");
    }

    public void UpdateOpenInventoryButton(Sprite itemIcon)
    {
        if(!IsOwner) return;

        selectedItemImage.sprite = itemIcon; //Show Icon of selected item
    }

    private void ToggleInventory()
    {
        if (playerInventoryUIBackground.activeSelf)
        {
            HideInventoryBackground();
        }
        else
        {
            ShowInventory();
        }
    }

    private void HideInventoryBackground()
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

    private void HideInventoryParent()
    {
        inventoryParent.gameObject.SetActive(false);
    }
    
    private void ShowInventoryParent()
    {
        inventoryParent.gameObject.SetActive(true);
    }

    private void ShowInventoryButton()
    {
        openInventoryBackground.SetActive(true);
    }

    public void InitializeOwner()
    {
        ShowInventoryParent();
        ShowInventory();
        ShowInventoryButton();
        tutorialParent.SetActive(true);
        tutorialButton.SetActive(true);
        // Debug.Log($"OWNERSHIP INVENTOR - {gameObject.transform.parent.parent.name}");
    }

    public void UnHandleInitializeOwner()
    {
        HideInventoryParent();
        HideInventoryBackground();
        HideInventoryButton();
    }
}
