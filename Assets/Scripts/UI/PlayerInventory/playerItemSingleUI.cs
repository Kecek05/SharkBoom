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
    [SerializeField] private TextMeshProUGUI ownerDebugText;
    [SerializeField] private TextMeshProUGUI itemCanBeUsedText;
    [SerializeField] private Button selectItemButton;

    private int myIndexItemInventory;

    private void Awake()
    {
        selectItemButton.onClick.AddListener(() =>
        {
            OnItemSingleSelected?.Invoke(myIndexItemInventory);
            Debug.Log($"Item {myIndexItemInventory} selected");
        });
    }

    public void Setup(string itemName, Image itemIcon, string itemCooldown, FixedString32Bytes ownerDebug, bool itemCanBeUsed, int indexItemInventory)
    {
        itemNameText.text = itemName;
        //itemImageIcon.sprite = itemIcon.sprite;
        itemCooldownText.text = itemCooldown;

        ownerDebugText.text = ownerDebug.ToString();
        itemCanBeUsedText.text = itemCanBeUsed.ToString();

        myIndexItemInventory = indexItemInventory;
    }

    private void Test()
    {

    }
}
