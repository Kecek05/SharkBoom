using System;
using Unity.Collections;
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

public struct ItemDataStruct : INetworkSerializable, IEquatable<ItemDataStruct>
{
    public int itemSOIndex;
    public int itemInventoryIndex;
    public bool itemCanBeUsed;
    public int itemCooldownRemaining;

    public FixedString32Bytes ownerDebug;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemSOIndex);
        serializer.SerializeValue(ref itemCanBeUsed);
        serializer.SerializeValue(ref itemCooldownRemaining);
        serializer.SerializeValue(ref ownerDebug);
        serializer.SerializeValue(ref itemInventoryIndex);
    }

    public bool Equals(ItemDataStruct other)
    {
        return itemSOIndex == other.itemSOIndex && itemCanBeUsed == other.itemCanBeUsed && itemCooldownRemaining == other.itemCooldownRemaining && ownerDebug == other.ownerDebug && itemInventoryIndex == other.itemInventoryIndex;
    }
}