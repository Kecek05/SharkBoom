using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{

    public GameObject itemClientPrefab;
    public GameObject itemServerPrefab;
    public string itemName;
    public Sprite itemIcon;
    [Space(5)]
    public float damage;
    public float mass;
    public int cooldown;
}
