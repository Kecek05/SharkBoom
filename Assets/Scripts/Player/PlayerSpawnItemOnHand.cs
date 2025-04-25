using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnItemOnHand : NetworkBehaviour
{
    public event Action<BaseItemThrowable> OnItemOnHandSpawned;
    public event Action<BaseItemThrowable> OnItemOnHandDespawned;
    public event Action<ItemSocket> OnItemSocketSelected;

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemSocket[] leftSideSockets;
    [SerializeField] private ItemSocket[] rightSideSockets;

    private ItemSocket selectedSocket;
    private int selectedItemSOIndex = 0;
    private bool isRightSocket = false; //Rotation that the player is looking
    private BaseItemThrowable spawnedItem;

    private bool canSpawnItem = false;

    public void HandleOnRotationChanged(bool isRight)
    {
        //Used to select the right side socket
        isRightSocket = isRight;
        UpdateSelectedSocket();
        SpawnItem();
    }

    public void HandleOnPlayerInventoryItemSelected(int selectedItemSOIndex)
    {
        //Based on the item select, save the item to spawn when drag start and select the corresponding socket based on item and on rotation
        this.selectedItemSOIndex = playerInventory.GetSelectedItemSOIndex();
        UpdateSelectedSocket();
    }

    public void HandleOnCrossfadeFinished()
    {
        Debug.Log("HandleOnPlayerAnimatorCrossfadeFinished");
        SpawnItem();
    }

    public void HandleOnPlayerStateChanged(PlayerState newState)
    {
        Debug.Log($"HandleOnPlayerStateMachineStateChanged: {newState}");


        canSpawnItem = false;

        switch (newState)
        {
            case PlayerState.IdleMyTurn:
                //Despawn item
                DespawnItem();
                break;
            case PlayerState.DraggingItem:
                //Spawn Item
                canSpawnItem = true;
                break;
            case PlayerState.DraggingJump:
                canSpawnItem = true;
                //Do nothing
                break;
            case PlayerState.DragReleaseItem:
                //Do nothing
                break;
            case PlayerState.DragReleaseJump:
                //Do nothing
                break;
        }
    }

    private void SpawnItem()
    {
        //Spawn selected Item on the selected socket
        if (!canSpawnItem) return; //Do nothing if the player is not in the right state

        //if (selectedInventoryIndexItem == null)
        //{
        //    Debug.LogWarning("Item not selected");
        //    return;
        //}

        if (spawnedItem != null)
        {
            Debug.LogWarning("Item already spawned, destroying it");
            spawnedItem.DestroyItem(() =>
            {
                spawnedItem = null;
                InstantiateObj();
                return;
            });
        } else
        {
            InstantiateObj();
        }
        
    }

    private void InstantiateObj()
    {
        if(!IsOwner) return; //Only the owner can spawn the item

        InstantiateObjServerRpc(NetworkManager.Singleton.LocalClientId, selectedSocket.transform.position, selectedItemSOIndex);

        //spawnedItem = Instantiate(selectedItemSO.itemClientPrefab, selectedSocket.transform.position, Quaternion.identity).GetComponent<BaseItemThrowable>();
        //spawnedItem.transform.SetParent(selectedSocket.transform);
        //spawnedItem.transform.localRotation = Quaternion.identity;

        //spawnedItem.Initialize();


    }

    [Rpc(SendTo.Server)]
    private void InstantiateObjServerRpc(ulong ownerClientId, Vector3 selectedSocketPos, int itemSOIndex)
    {
        NetworkObject spawnedItemNetworkObject = Instantiate(playerInventory.GetItemSOByItemSOIndex(itemSOIndex).itemClientPrefab, selectedSocketPos, Quaternion.identity).GetComponent<NetworkObject>();
        spawnedItemNetworkObject.Spawn();
        spawnedItemNetworkObject.ChangeOwnership(ownerClientId);

        CallOnItemOnHandClientRpc(spawnedItemNetworkObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallOnItemOnHandClientRpc(NetworkObjectReference itemNetworkObject)
    {
        if(itemNetworkObject.TryGet(out NetworkObject itemNetworkObjectRef))
        {
            spawnedItem = itemNetworkObjectRef.GetComponent<BaseItemThrowable>();
        } else
        {
            Debug.LogWarning("Item not found");
            return;
        }

        if(IsOwner)
        {
            spawnedItem.Initialize(selectedSocket.transform);
            spawnedItem.transform.localRotation = Quaternion.identity;
        }

        OnItemOnHandSpawned?.Invoke(spawnedItem);
    }

    public void HandleOnShoot()
    {
        //Release item
        if (spawnedItem != null)
        {
            spawnedItem = null;
        }
    }

    private void DespawnItem()
    {
        //Despawn item
        if (spawnedItem != null)
        {
            spawnedItem.DestroyItem(() =>
            {
                OnItemOnHandDespawned?.Invoke(spawnedItem);
                spawnedItem = null;
            });
            
        }

    }

    private void UpdateSelectedSocket()
    {

        if (isRightSocket)
        {
            foreach (ItemSocket socket in rightSideSockets)
            {
                if (socket.ItemSO == playerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex))
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                    OnItemSocketSelected?.Invoke(selectedSocket);
                }
            }
        } else
        {
            foreach (ItemSocket socket in leftSideSockets)
            {
                if (socket.ItemSO == playerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex))
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                    OnItemSocketSelected?.Invoke(selectedSocket);
                }
            }
        }


    }
}
