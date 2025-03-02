using System;
using UnityEngine;

public class playerInventoryUI : MonoBehaviour
{


    private void Awake()
    {
        PlayerInventory.OnItemChanged += PlayerInventory_OnItemChanged;
    }

    private void PlayerInventory_OnItemChanged(ItemDataStruct @struct)
    {
        Debug.Log("UI Inventory");
    }
}
