using System;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Item on inventory data
/// </summary>
public struct ItemInventoryData : INetworkSerializable, IEquatable<ItemInventoryData>
{
    /// <summary>
    /// ID of the item in the ItemSO, Primary key
    /// </summary>
    public int itemID;

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
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref itemCanBeUsed);
        serializer.SerializeValue(ref itemCooldownRemaining);
    }

    public bool Equals(ItemInventoryData other)
    {
        //return itemSOIndex == other.itemSOIndex && itemCanBeUsed == other.itemCanBeUsed && itemCooldownRemaining == other.itemCooldownRemaining && ownerDebug == other.ownerDebug && itemInventoryIndex == other.itemInventoryIndex;
        return itemID == other.itemID;
    }
}


/// <summary>
/// Data to launch an item
/// </summary>
public struct ItemLauncherData : INetworkSerializable, IEquatable<ItemLauncherData>
{

    /// <summary>
    /// Force of the drag
    /// </summary>
    public float dragForce;

    /// <summary>
    /// Direction of the drag
    /// </summary>
    public Vector2 dragDirection;


    /// <summary>
    /// ID to get the itemSO from the ItemsListSO
    /// </summary>
    public int selectedItemID;

    /// <summary>
    /// Owner of the Item launched
    /// </summary>
    public PlayableState ownerPlayableState;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref dragForce);
        serializer.SerializeValue(ref dragDirection);
        serializer.SerializeValue(ref selectedItemID);
        serializer.SerializeValue(ref ownerPlayableState);
    }

    public bool Equals(ItemLauncherData other)
    {
        return dragForce == other.dragForce && dragDirection == other.dragDirection && selectedItemID == other.selectedItemID && ownerPlayableState == other.ownerPlayableState;
    }
}
