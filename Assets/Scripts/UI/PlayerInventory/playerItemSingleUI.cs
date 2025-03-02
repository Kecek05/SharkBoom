using Sortify;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class playerItemSingleUI : MonoBehaviour
{
    [BetterHeader("References")]

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImageIcon;
    [SerializeField] private TextMeshProUGUI itemCooldownText;
    [SerializeField] private TextMeshProUGUI ownerDebugText;
    [SerializeField] private TextMeshProUGUI itemCanBeUsedText;
    [SerializeField] private Button selectItemButton;

    private int myIndexItemSO;

    private void Awake()
    {
        selectItemButton.onClick.AddListener(() =>
        {
            Debug.Log($"Item {myIndexItemSO} selected");
        });
    }

    public void Setup(string itemName, Image itemIcon, string itemCooldown, FixedString32Bytes ownerDebug, bool itemCanBeUsed, int indexItemSO)
    {
        itemNameText.text = itemName;
        //itemImageIcon.sprite = itemIcon.sprite;
        itemCooldownText.text = itemCooldown;

        ownerDebugText.text = ownerDebug.ToString();
        itemCanBeUsedText.text = itemCanBeUsed.ToString();

        myIndexItemSO = indexItemSO;
    }
}
