using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{

    public GameObject itemClientPrefab;
    public GameObject itemServerPrefab;
    public string itemName;
    public Image itemIcon;
    [Space(5)]
    public float damage;
    public Rigidbody rb;

    /// <summary>
    /// Numbers of turns that need to wait to use the item again
    /// </summary>
    public int cooldown;
}
