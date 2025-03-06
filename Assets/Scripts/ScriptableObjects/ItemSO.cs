using Sortify;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{

    public GameObject itemClientPrefab;
    public GameObject itemServerPrefab;
    public string itemName;
    public Image itemIcon;
    public Rigidbody rb;
    [Space(5)]

    [BetterHeader("Item Settings", 12)]
    public DamageableSO damageableSO;

    /// <summary>
    /// Numbers of turns that need to wait to use the item again
    /// </summary>
    public int cooldown;


}
