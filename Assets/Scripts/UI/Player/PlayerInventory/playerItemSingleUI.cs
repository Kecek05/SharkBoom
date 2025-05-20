using Sortify;
using System;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemSingleUI : MonoBehaviour
{
    public event Action<int> OnItemSingleSelected;

    [BetterHeader("References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImageIcon;
    [SerializeField] private TextMeshProUGUI itemCooldownText;
    //[SerializeField] private TextMeshProUGUI itemCanBeUsedText;
    [SerializeField] private Button selectThisItemButton;
    //[SerializeField] private TextMeshProUGUI itemInventoryIndexText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject itemCanBeUsedObj;
    [SerializeField] private TextMeshProUGUI itemDamageText;
    private PlayerInventoryUI playerInventoryUI;

    private int myIndexItemInventory;
    public int ItemIndex => myIndexItemInventory;

    [BetterHeader("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unSelectedColor;

    private void Awake()
    {
        selectThisItemButton.onClick.AddListener(() =>
        {
            playerInventoryUI.SelecItem(myIndexItemInventory);
        });
    }

    public void Setup(string itemName, Sprite itemIcon, string itemCooldown, bool itemCanBeUsed, int indexItemInventory, float itemDamage ,PlayerInventoryUI _playerInventoryUI)
    {
        itemNameText.text = itemName;
        itemImageIcon.sprite = itemIcon;
        itemCooldownText.text = itemCooldown;
        itemDamageText.text = itemDamage.ToString();

        //itemCanBeUsedText.text = itemCanBeUsed.ToString();

        myIndexItemInventory = indexItemInventory;

        //itemInventoryIndexText.text = indexItemInventory.ToString();

        playerInventoryUI = _playerInventoryUI;
    }

    public void UpdateCooldown(int newCooldown)
    {
        itemCooldownText.text = newCooldown.ToString();
    }

    public void UpdateCanBeUsed(bool itemCanBeUsed)
    {
        //itemCanBeUsedText.text = itemCanBeUsed.ToString();
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

    public int GetIndexItemInventory() { return myIndexItemInventory; }
    public PlayerInventoryUI GetPlayerInventoryUI() {  return playerInventoryUI; }

}
