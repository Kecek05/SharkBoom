using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class playerItemSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImageIcon;
    [SerializeField] private TextMeshProUGUI itemCooldownText;
    [SerializeField] private TextMeshProUGUI ownerDebugText;
    public void Setup(string itemName, Image itemIcon, string itemCooldown, FixedString32Bytes ownerDebug)
    {
        itemNameText.text = itemName;
        //itemImageIcon.sprite = itemIcon.sprite;
        itemCooldownText.text = itemCooldown;

        ownerDebugText.text = ownerDebug.ToString();
    }
}
