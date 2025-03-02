using System;
using Unity.Netcode;
using UnityEngine;

public class ItemData  //INetworkSerializable, IEquatable<ItemData>
{
    //Responsable to save the Item data in the players inventory. Ammo, Item and etc


    /// <summary>
    /// index to get the itemSO from the ItemsListSO
    /// </summary>
    public int itemSOIndex;

    /// <summary>
    /// How many uses the item has left
    /// </summary>
    //public int itemUsesLeft;

    /// <summary>
    /// If the item can be used or not
    /// </summary>
    public bool itemCanBeUsed;

    /// <summary>
    /// The remaining cooldown of the item, if 0, the item can be used
    /// </summary>
    public int itemCooldownRemaining;
}