using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemSingleUI : MonoBehaviour
{
    public event Action<int> OnItemSingleSelected;

    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImageIcon;
    [SerializeField] private TextMeshProUGUI itemCooldownText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject itemCanBeUsedObj;
    [SerializeField] private TextMeshProUGUI itemDamageText;
    private PlayerInventoryUI playerInventoryUI;

    private int myItemID;
    public int MyItemID => myItemID;

    [BetterHeader("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unSelectedColor;

    public void SelectThisItem()
    {
        playerInventoryUI.SelecItem(myItemID);
    }

    public void Setup(string itemName, Sprite itemIcon, int itemCooldown, bool itemCanBeUsed, int itemID, float itemDamage ,PlayerInventoryUI _playerInventoryUI)
    {
        itemNameText.text = itemName;
        itemImageIcon.sprite = itemIcon;
        UpdateCooldown(itemCooldown);
        itemDamageText.text = itemDamage.ToString();
        myItemID = itemID;
        playerInventoryUI = _playerInventoryUI;

        UnSelectedThisItem();
        UpdateCanBeUsed(true);
    }

    public void UpdateCooldown(int newCooldown)
    {
        if(newCooldown <= 0)
        {
            itemCooldownText.gameObject.SetActive(false);
        } else
        {
            itemCooldownText.text = newCooldown.ToString();
            itemCooldownText.gameObject.SetActive(true);
        }
    }

    public void UpdateCanBeUsed(bool itemCanBeUsed)
    {
        if(itemCanBeUsed)
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
        backgroundImage.color = selectedColor;
    }

    public void UnSelectedThisItem()
    {
        backgroundImage.color = unSelectedColor;
    }

    public string GetItemName() { return itemNameText.text; } 

    public Sprite GetItemIcon() { return itemImageIcon.sprite; }

    public string GetItemCooldown() { return itemCooldownText.text; }

    public int GetIndexItemInventory() { return myItemID; }
    public PlayerInventoryUI GetPlayerInventoryUI() {  return playerInventoryUI; }

}
