using System;
using System.Collections.Generic;
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
    
    public Vector3 shootPosition;
    
    public Quaternion shootRotation;
    
    public bool isRightSocket;
    
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref dragForce);
        serializer.SerializeValue(ref dragDirection);
        serializer.SerializeValue(ref selectedItemID);
        serializer.SerializeValue(ref ownerPlayableState);
        serializer.SerializeValue(ref shootPosition);
        serializer.SerializeValue(ref shootRotation);
        serializer.SerializeValue(ref isRightSocket);
    }

    public bool Equals(ItemLauncherData other)
    {
        return dragForce == other.dragForce && dragDirection == other.dragDirection && selectedItemID == other.selectedItemID && ownerPlayableState == other.ownerPlayableState && shootPosition == other.shootPosition && shootRotation == other.shootRotation && isRightSocket == other.isRightSocket;
    }
}

/// <summary>
/// Used to reconcile items on the network when activating an item, contains all the data needed to reconstruct an item 
/// </summary>
public struct ItemReconcileData : INetworkSerializable, IEquatable<ItemReconcileData>
{
    public int itemID;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;
    public Vector3 position;
    public Quaternion rotation;
    public bool haveDisableCollisionComponent;
    public bool isCollidersEnabled;
    public bool haveHideMeshComponent;
    public bool isMeshVisible;
    public bool isKinematic;
    public bool haveSpinObjectComponent;
    public bool isSpinning;
    
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref linearVelocity);
        serializer.SerializeValue(ref angularVelocity);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref isCollidersEnabled);
        serializer.SerializeValue(ref isMeshVisible);
        serializer.SerializeValue(ref isKinematic);
        serializer.SerializeValue(ref isSpinning);
        serializer.SerializeValue(ref haveDisableCollisionComponent);
        serializer.SerializeValue(ref haveHideMeshComponent);
        serializer.SerializeValue(ref haveSpinObjectComponent);

    }

    public bool Equals(ItemReconcileData other)
    {
        return itemID == other.itemID &&
               linearVelocity.Equals(other.linearVelocity) &&
               angularVelocity.Equals(other.angularVelocity) &&
               position.Equals(other.position) &&
               rotation.Equals(other.rotation) &&
               isCollidersEnabled == other.isCollidersEnabled &&
               isMeshVisible == other.isMeshVisible &&
               isKinematic == other.isKinematic &&
               isSpinning == other.isSpinning
               && haveDisableCollisionComponent == other.haveDisableCollisionComponent &&
               haveHideMeshComponent == other.haveHideMeshComponent &&
               haveSpinObjectComponent == other.haveSpinObjectComponent;
    }
}
