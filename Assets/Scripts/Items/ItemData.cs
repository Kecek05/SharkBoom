using System;
using Unity.Netcode;
using UnityEngine;

public class ItemData  //INetworkSerializable, IEquatable<ItemData>
{
    //Responsable to save the Item data in the players inventory. Ammo, Item and etc

    public ItemSO itemSO;
    public int itemIndex;
    public int itemUsesLeft;
    public bool itemCanBeUsed;



    //public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //{
    //    serializer.SerializeValue(ref itemIndex);
    //    serializer.SerializeValue(ref itemUsesLeft);
    //    serializer.SerializeValue(ref itemCanBeUsed);
    //}

    //public bool Equals(ItemData other)
    //{
    //    return itemIndex == other.itemIndex && itemUsesLeft == other.itemUsesLeft && itemCanBeUsed == other.itemCanBeUsed;
    //}
}
