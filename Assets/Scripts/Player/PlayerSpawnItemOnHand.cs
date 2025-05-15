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
        if (!IsOwner) return;
        //Used to select the right side socket
        isRightSocket = isRight;
        UpdateSelectedSocket();
        SpawnItem();
    }

    public void HandleOnPlayerInventoryItemSelected(int selectedItemSOIndex)
    {
        if (!IsOwner) return;
        //Based on the item select, save the item to spawn when drag start and select the corresponding socket based on item and on rotation
        this.selectedItemSOIndex = playerInventory.GetSelectedItemSOIndex();
        UpdateSelectedSocket();
    }

    public void HandleOnCrossfadeFinished()
    {
        if (!IsOwner) return;
        SpawnItem();
    }

    public void HandleOnPlayerStateChanged(PlayerState newState)
    {
        if (!IsOwner) return;
        canSpawnItem = false;

        switch (newState)
        {
            case PlayerState.IdleMyTurn:
                DespawnItem();
                break;
            case PlayerState.DraggingItem:
                canSpawnItem = true;
                break;
            case PlayerState.DraggingJump:
                canSpawnItem = true;
                break;
            case PlayerState.DragReleaseItem:
                HandleOnShoot();
                break;
            case PlayerState.DragReleaseJump:
                HandleOnShoot();
                break;
        }
    }

    private void SpawnItem()
    {
        //Spawn selected Item on the selected socket
        if (!canSpawnItem) return; //Do nothing if the player is not in the right state

        if (spawnedItem != null)
        {
            spawnedItem.ChangeFollowTransform(selectedSocket.transform);
        }
        else
        {
            //it's null
            InstantiateObj();
        }
        
    }

    private void InstantiateObj()
    {
        if(!IsOwner) return; //Only the owner can spawn the item
        UpdateSelectedSocket();
        InstantiateObjServerRpc(NetworkManager.Singleton.LocalClientId, selectedSocket.transform.position, selectedItemSOIndex);

    }

    [Rpc(SendTo.Server)]
    private void InstantiateObjServerRpc(ulong ownerClientId, Vector3 selectedSocketPos, int itemSOIndex)
    {
        NetworkObject spawnedItemNetworkObject = Instantiate(playerInventory.GetItemSOByItemSOIndex(itemSOIndex).itemPrefab, selectedSocketPos, Quaternion.identity).GetComponent<NetworkObject>();
        spawnedItemNetworkObject.Spawn();
        spawnedItemNetworkObject.ChangeOwnership(ownerClientId);

        if(IsServer && !IsHost) //Only DS, cuz host will get the ref in the CallOnItemOnHandClientRpc
            spawnedItem = spawnedItemNetworkObject.GetComponent<BaseItemThrowable>();

        CallOnItemOnHandClientRpc(spawnedItemNetworkObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallOnItemOnHandClientRpc(NetworkObjectReference itemNetworkObject)
    {
        if(itemNetworkObject.TryGet(out NetworkObject itemNetworkObjectRef))
        {
            if (spawnedItem != null)
                spawnedItem.DestroyItem();

            spawnedItem = itemNetworkObjectRef.GetComponent<BaseItemThrowable>();

            if(IsOwner)
            {
                spawnedItem.Initialize(selectedSocket.transform);
                spawnedItem.transform.localRotation = Quaternion.identity;
            }

        } else
        {
            Debug.LogWarning("Item not found");
            return;
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
