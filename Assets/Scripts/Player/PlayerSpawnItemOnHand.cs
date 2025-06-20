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
    private int selectedItemID => playerInventory.SelectedItemID;
    private bool isRightSocket = false; //Rotation that the player is looking
    private BaseItemThrowable spawnedItem;

    private bool canSpawnItem = false;
    private bool itemAlreadySpawned = false;

    //Publics
    public Transform SelectedSocketTransform => selectedSocket.transform;
    public BaseItemThrowable SpawnedItem => spawnedItem;
    
    public void HandleOnRotationChanged(bool isRight)
    {
        if(!IsOwner) return;
        
        //Used to select the right side socket
        isRightSocket = isRight;
        UpdateSelectedSocket(isRightSocket, playerInventory.SelectedItemID);
        SpawnItem(selectedItemID);
    }

    public void HandleOnPlayerInventoryItemSelected(int selectedItemSOIndex)
    {
        //if (!IsOwner) return;
        //Based on the item select, save the item to spawn when drag start and select the corresponding socket based on item and on rotation
        UpdateSelectedSocket(isRightSocket, playerInventory.SelectedItemID);
    }

    public void HandleOnCrossfadeFinished()
    {
        //if (!IsOwner) return;
        SpawnItem(selectedItemID);
    }

    public void HandleOnPlayerStateChanged(PlayerState newState)
    {
        if (!IsOwner) return;
        
        //canSpawnItem = false;
        switch (newState)
        {
            case PlayerState.IdleMyTurn:
                itemAlreadySpawned = false;
                canSpawnItem = false;
                DespawnItem();
                break;
            case PlayerState.DraggingItem:
                canSpawnItem = true;
                break;
            case PlayerState.DraggingJump:
                canSpawnItem = true;
                break;
            case PlayerState.DragReleaseItem:
                canSpawnItem = true;
                //HandleOnShoot();
                break;
            case PlayerState.DragReleaseJump:
                canSpawnItem = true;
                //HandleOnShoot();
                break;
            case PlayerState.MyTurnEnded:
                canSpawnItem = false;
                DespawnItem();
                break;
            case PlayerState.IdleEnemyTurn:
                canSpawnItem = false;
                DespawnItem();
                break;
        }
    }

    private void SpawnItem(int _selectedItemID)
    {
        //Spawn selected Item on the selected socket
        if (!canSpawnItem) return; //Do nothing if the player is not in the right state
        
        UpdateSpawnedItem(_selectedItemID);
        
        // if(IsOwner)
        //     SpawnItemServerRpc(_selectedItemID);
    }

    /// <summary>
    /// Called from the Client, to spawn the item recieved from the itemLauncherData
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="isRightSocket"></param>
    public void SpawnItemClient(int itemID, bool isRightSocket)
    {
        UpdateSelectedSocket(isRightSocket, itemID);
        InstantiateLocalObj(itemID);
    }
    
    private void UpdateSpawnedItem(int _selectedItemID)
    {
        UpdateSelectedSocket(isRightSocket, playerInventory.SelectedItemID);
        
        if (spawnedItem)
        {
            spawnedItem.ChangeFollowTransform(selectedSocket.transform);
        }
        else
        {
            //it's null
            TrySpawnItem(_selectedItemID);
        }
    }

    private void TrySpawnItem(int _selectedItemID)
    {
        if(itemAlreadySpawned) return; //item already spawned this turn
        
        itemAlreadySpawned = true;
        
        InstantiateLocalObj(_selectedItemID);
    }

    private void InstantiateLocalObj(int _selectedItemID)
    {
        GameObject spawnedItemObject = ObjectPool.Instance.GetObject(_selectedItemID, selectedSocket.transform.position, Quaternion.identity);
        spawnedItem = spawnedItemObject.GetComponent<BaseItemThrowable>();
        
        spawnedItem.Initialize(selectedSocket.transform);
        spawnedItem.transform.localRotation = Quaternion.identity;
        
        OnItemOnHandSpawned?.Invoke(spawnedItem);
    }

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

    private void UpdateSelectedSocket(bool isRight, int selectedItemID)
    {
        if (isRight)
        {
            foreach (ItemSocket socket in rightSideSockets)
            {
                if (socket.ItemSO.itemID == selectedItemID)
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
                if (socket.ItemSO.itemID == selectedItemID)
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                    OnItemSocketSelected?.Invoke(selectedSocket);
                }
            }
        }
    }
}
