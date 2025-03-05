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
    [SerializeField] private TextMeshProUGUI itemCanBeUsedText;
    [SerializeField] private Button selectThisItemButton;
    [SerializeField] private TextMeshProUGUI itemInventoryIndexText;
    [SerializeField] private Image backgroundImage;
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

    public void Setup(string itemName, Image itemIcon, string itemCooldown, bool itemCanBeUsed, int indexItemInventory, PlayerInventoryUI _playerInventoryUI)
    {
        itemNameText.text = itemName;
        //itemImageIcon.sprite = itemIcon.sprite;
        itemCooldownText.text = itemCooldown;

        itemCanBeUsedText.text = itemCanBeUsed.ToString();

        myIndexItemInventory = indexItemInventory;

        itemInventoryIndexText.text = indexItemInventory.ToString();

        playerInventoryUI = _playerInventoryUI;
    }

    public void UpdateCooldown(string newCooldown)
    {
        itemCooldownText.text = newCooldown;
    }

    public void UpdateCanBeUsed(bool itemCanBeUsed)
    {
        itemCanBeUsedText.text = itemCanBeUsed.ToString();
    }

    public void SelectedThisItem()
    {
        backgroundImage.color = selectedColor;
    }

    public void UnSelectedThisItem()
    {
        backgroundImage.color = unSelectedColor;
    }
}
