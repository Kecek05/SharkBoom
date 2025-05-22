using Sortify;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public int itemIndex;
    public GameObject itemPrefab;
    public string itemName;
    public Sprite itemIcon;
    public Rigidbody rb;
    [Space(5)]

    [BetterHeader("Item Settings", 12)]
    public DamageableSO damageableSO;

    /// <summary>
    /// Numbers of turns that need to wait to use the item again | 1 = Next round will be available | 2 = one round unavailable
    /// </summary>
    [Tooltip("1 = Next round will be available | 2 = one round unavailable")]
    public int cooldown;

    public ItemAnimationSO itemAnimationSO;

}
