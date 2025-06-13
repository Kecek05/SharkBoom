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
    private int selectedItemID = 0;
    private bool isRightSocket = false; //Rotation that the player is looking
    private BaseItemThrowable spawnedItem;

    private bool canSpawnItem = false;

    //Publics
    public Transform SelectedSocketTransform => selectedSocket.transform;
    
    //DEBUG
    public BaseItemThrowable SpawnedItem => spawnedItem;
    
    public void HandleOnRotationChanged(bool isRight)
    {
        //if (!IsOwner) return;
        //Used to select the right side socket
        isRightSocket = isRight;
        UpdateSelectedSocket();
        //SpawnItem(selectedItemID);
        TriggerSpawnItem(selectedItemID);
    }

    public void HandleOnPlayerInventoryItemSelected(int selectedItemSOIndex)
    {
        //if (!IsOwner) return;
        //Based on the item select, save the item to spawn when drag start and select the corresponding socket based on item and on rotation
        selectedItemID = selectedItemSOIndex;
        UpdateSelectedSocket();
    }

    public void HandleOnCrossfadeFinished()
    {
        //if (!IsOwner) return;
        SpawnItem(selectedItemID);
    }

    public void HandleOnPlayerStateChanged(PlayerState newState)
    {
        //if (!IsOwner) return;
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
            case PlayerState.MyTurnEnded:
                DespawnItem();
                break;
        }
    }

    private void SpawnItem(int _selectedItemID)
    {
        //Spawn selected Item on the selected socket
        if (!canSpawnItem) return; //Do nothing if the player is not in the right state
        
        TriggerSpawnItem(_selectedItemID);
        
        if(IsOwner)
            SpawnItemServerRpc(_selectedItemID);
    }
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void SpawnItemServerRpc(int _selectedItemID)
    {
        SpawnItemClientRpc(_selectedItemID);
    }
    
    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void SpawnItemClientRpc(int _selectedItemID)
    {
        TriggerSpawnItem(_selectedItemID);
    }
    
    private void TriggerSpawnItem(int _selectedItemID)
    {
        UpdateSelectedSocket();
        
        if (spawnedItem)
        {
            spawnedItem.ChangeFollowTransform(selectedSocket.transform);
        }
        else
        {
            //it's null
            InstantiateLocalObj(_selectedItemID);
        }
    }

   /* private void InstantiateObj()
    {
        UpdateSelectedSocket();
        InstantiateObjServerRpc(NetworkManager.Singleton.LocalClientId, selectedSocket.transform.position, selectedItemSOIndex);

    }*/

    private void InstantiateLocalObj(int _selectedItemID)
    {
        GameObject spawnedItemObject = ObjectPool.Instance.GetObject(_selectedItemID, selectedSocket.transform.position, Quaternion.identity);
        spawnedItem = spawnedItemObject.GetComponent<BaseItemThrowable>();
        
        spawnedItem.Initialize(selectedSocket.transform);
        spawnedItem.transform.localRotation = Quaternion.identity;
        
        OnItemOnHandSpawned?.Invoke(spawnedItem);
    }
    /*
    [Rpc(SendTo.Server)]
    private void InstantiateObjServerRpc(ulong ownerClientId, Vector3 selectedSocketPos, int itemSOIndex)
    {
        NetworkObject spawnedItemNetworkObject = ObjectPool.Instance.GetObject(playerInventory.GetItemSOByItemSOIndex(itemSOIndex).itemIndex, selectedSocketPos, Quaternion.identity);
        spawnedItemNetworkObject.Spawn();
        spawnedItemNetworkObject.ChangeOwnership(ownerClientId);

        if(IsServer && !IsHost) //Only DS, cuz host will get the ref in the CallOnItemOnHandClientRpc
            spawnedItem = spawnedItemNetworkObject.GetComponent<BaseItemThrowable>();

        CallOnItemOnHandClientRpc(spawnedItemNetworkObject);
    }*/

    /*[Rpc(SendTo.ClientsAndHost)]
    private void CallOnItemOnHandClientRpc(NetworkObjectReference itemNetworkObject)
    {
        if(itemNetworkObject.TryGet(out NetworkObject itemNetworkObjectRef))
        {
            if (spawnedItem)
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
    }*/

    public void HandleOnShoot()
    {
        //Release item
        spawnedItem = null;
    }

    private void DespawnItem()
    {
        //Despawn item
        if (spawnedItem)
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
                if (socket.ItemSO.itemID == playerInventory.SelectedItemID)
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
                if (socket.ItemSO.itemID == playerInventory.SelectedItemID)
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                    OnItemSocketSelected?.Invoke(selectedSocket);
                }
            }
        }
    }
}
