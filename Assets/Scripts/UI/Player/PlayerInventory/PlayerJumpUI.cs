using Sortify;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJumpUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject itemCanBeUsedObj;
    [SerializeField] private Image itemImageIcon;

    [BetterHeader("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unSelectedColor;
    private const int JUMP_ITEM_INVENTORY_INDEX = 0; //Jump Index


    public void HandleOnPlayerInventoryItemSelected(int itemInventoryIndex)
    {
        if (!IsOwner) return;

        if (JUMP_ITEM_INVENTORY_INDEX == itemInventoryIndex)
        {
            SelectedThisItem();
        }
        else
        {
            UnSelectedThisItem();
        }
    }

    public void HandleOnPlayerInventoryItemChanged(ItemInventoryData itemData)
    {
        if (!IsOwner) return;

        if (JUMP_ITEM_INVENTORY_INDEX == itemData.itemInventoryIndex)
        {
            UpdateCanBeUsed(itemData.itemCanBeUsed);
        }
    }


    public void UpdateCanBeUsed(bool itemCanBeUsed)
    {
        if (itemCanBeUsed)
        {
            itemCanBeUsedObj.SetActive(false);
        }
        else
        {
            itemCanBeUsedObj.SetActive(true);
        }
    }

    public void SelectedThisItem()
    {
        itemImageIcon.color = selectedColor;
    }

    public void UnSelectedThisItem()
    {
        itemImageIcon.color = unSelectedColor;
    }
}
