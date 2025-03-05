using System;
using Unity.Collections;
using Unity.Netcode;

public struct ItemDataStruct : INetworkSerializable, IEquatable<ItemDataStruct>
{

    /// <summary>
    /// index to get the itemSO from the ItemsListSO
    /// </summary>
    public int itemSOIndex;

    /// <summary>
    /// index to get the itemDataStruct from the playerInventory, Primary key
    /// </summary>
    public int itemInventoryIndex;

    /// <summary>
    /// If the item can be used or not
    /// </summary>
    public bool itemCanBeUsed;

    /// <summary>
    /// The remaining cooldown of the item, if 0, the item can be used
    /// </summary>
    public int itemCooldownRemaining;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemSOIndex);
        serializer.SerializeValue(ref itemCanBeUsed);
        serializer.SerializeValue(ref itemCooldownRemaining);
        serializer.SerializeValue(ref itemInventoryIndex);
    }

    public bool Equals(ItemDataStruct other)
    {
        //return itemSOIndex == other.itemSOIndex && itemCanBeUsed == other.itemCanBeUsed && itemCooldownRemaining == other.itemCooldownRemaining && ownerDebug == other.ownerDebug && itemInventoryIndex == other.itemInventoryIndex;
        return itemInventoryIndex == other.itemInventoryIndex;
    }
}