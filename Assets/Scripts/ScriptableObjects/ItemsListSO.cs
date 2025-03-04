using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemsListSO", menuName = "Scriptable Objects/ItemsListSO")]
public class ItemsListSO : ScriptableObject
{
    public List<ItemSO> allItemsSOList;

}
