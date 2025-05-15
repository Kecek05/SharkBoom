using Unity.Netcode;
using UnityEngine;

public class PlayerAnimatorTest : NetworkBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    [SerializeField] private PlayerLauncherTest playerLauncherTest;

    [SerializeField] private ItemsListSO itemSOList;

    private ItemSO itemSelectedSO;

    [SerializeField] private Transform spawnPos;

    private GameObject lastGameObject;

    public override void OnNetworkSpawn()
    {
        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;
        //playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        //playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        Debug.Log("PlayerAnimatorTest started");
        //Start with Idle
        SetIsRight(false);
        SetPlayerStateByIndex(0);
    }

    private void InstantiateObj()
    {
        if (!IsOwner) return; //Only the owner can spawn the item

        if(lastGameObject != null)
        {
            lastGameObject.GetComponent<NetworkObject>().Despawn();
            lastGameObject = null;
        }

        InstantiateObjServerRpc(NetworkManager.Singleton.LocalClientId, spawnPos.position);

        //spawnedItem = Instantiate(selectedItemSO.itemClientPrefab, selectedSocket.transform.position, Quaternion.identity).GetComponent<BaseItemThrowable>();
        //spawnedItem.transform.SetParent(selectedSocket.transform);
        //spawnedItem.transform.localRotation = Quaternion.identity;

        //spawnedItem.Initialize();


    }

    [Rpc(SendTo.Server)]
    private void InstantiateObjServerRpc(ulong ownerClientId, Vector3 selectedSocketPos)
    {
        NetworkObject spawnedItemNetworkObject = Instantiate(itemSelectedSO.itemPrefab, selectedSocketPos, Quaternion.identity).GetComponent<NetworkObject>();
        lastGameObject = spawnedItemNetworkObject.gameObject;
        spawnedItemNetworkObject.Spawn();
        spawnedItemNetworkObject.ChangeOwnership(ownerClientId);

        CallOnItemOnHandClientRpc(spawnedItemNetworkObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CallOnItemOnHandClientRpc(NetworkObjectReference itemNetworkObject)
    {
        if (itemNetworkObject.TryGet(out NetworkObject itemNetworkObjectRef))
        {
            BaseItemThrowable item = itemNetworkObjectRef.GetComponent<BaseItemThrowable>();


            if (IsOwner)
            {
                item.Initialize(spawnPos);
                item.transform.localRotation = Quaternion.identity;
            }
        }
    }


    private void HandleOnPlayerAnimatorCrossfadeFinished()
    {
        //playerSpawnItemOnHand.HandleOnCrossfadeFinished();
    }

    public void ButtonSelectItem(int index)
    {
        if (index < 0 || index >= itemSOList.allItemsSOList.Count)
        {
            Debug.LogWarning("Index out of range");
            return;
        }
        itemSelectedSO = itemSOList.allItemsSOList[index];
        HandleOnItemSelectedSO(itemSelectedSO);
        InstantiateObj();
    }


    public void SetPlayerStateByIndex(int playerStateIndex)
    {

        switch(playerStateIndex)
        {
            case 0:
                HandleOnPlayerStateMachineStateChanged(PlayerState.IdleMyTurn);
                break;
            case 1:
                if(itemSelectedSO == null)
                {
                    Debug.LogWarning("Item not selected");
                    return;
                }
                HandleOnPlayerStateMachineStateChanged(PlayerState.DraggingItem);
                break;
            case 2:
                HandleOnPlayerStateMachineStateChanged(PlayerState.DraggingJump);
                break;
            case 3:
                if (itemSelectedSO == null)
                {
                    Debug.LogWarning("Item not selected");
                    return;
                }
                HandleOnPlayerStateMachineStateChanged(PlayerState.DragReleaseItem);
                break;
            case 4:
                HandleOnPlayerStateMachineStateChanged(PlayerState.DragReleaseJump);
                break;
        }
    }

    public void SetIsRight(bool isRight)
    {
        HandleOnRotationChanged(isRight);
    }

    private void HandleOnItemSelectedSO(ItemSO itemSelectedSO)
    {
        playerAnimator.HandleOnItemSelectedSO(itemSelectedSO);
       // playerSpawnItemOnHand.HandleOnPlayerInventoryItemSelected(itemSelectedSO);
    }

    private void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        playerAnimator.HandleOnPlayerStateMachineStateChanged(newState);
        //playerSpawnItemOnHand.HandleOnPlayerStateChanged(newState);
    }

    private void HandleOnRotationChanged(bool isRight)
    {
        playerAnimator.HandleOnRotationChanged(isRight);
        //playerSpawnItemOnHand.HandleOnRotationChanged(isRight);
    }

    private void HandleOnPlayerSpawnItemOnHandItemOnHandSpawned(BaseItemThrowable throwable)
    {
        playerLauncherTest.HandleOnItemOnHandSpawned(throwable);
    }

    private void HandleOnPlayerSpawnItemOnHandItemOnHandDespawned(BaseItemThrowable throwable)
    {
        playerLauncherTest.HandleOnItemOnHandDespawned(throwable);
    }


}
