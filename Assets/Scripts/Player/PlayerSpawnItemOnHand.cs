using Sortify;
using UnityEngine;

public class PlayerSpawnItemOnHand : MonoBehaviour
{

    [SerializeField] private ItemSocket[] leftSideSockets;
    [SerializeField] private ItemSocket[] rightSideSockets;

    private ItemSocket selectedSocket;
    private ItemSO selectedItemSO;
    private bool isRightSocket; //Rotation that the player is looking
    private Transform spawnedItem;

    public void HandleOnRotationChanged(bool isRight)
    {
        //Used to select the right side socket
        isRightSocket = isRight;
        UpdateSelectedSocket();
        SpawnItem();
    }

    public void HandleOnItemSelectedSO(ItemSO itemSelectedSO)
    {
        //Based on the item select, save the item to spawn when drag start and select the corresponding socket based on item and on rotation
        selectedItemSO = itemSelectedSO;
        UpdateSelectedSocket();
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.IdleMyTurn:
                //Despawn item
                DespawnItem();
                break;
            case PlayerState.DraggingItem:
                //Spawn Item
                SpawnItem();
                break;
            case PlayerState.DraggingJump:
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

        if(selectedItemSO == null)
        {
            Debug.LogWarning("Item not selected");
            return;
        }

        if (spawnedItem != null)
        {
            Debug.LogWarning("Item already spawned, destroying it");
            Destroy(spawnedItem);
            spawnedItem = null;
        }

        GameObject itemSpawned = Instantiate(selectedItemSO.itemClientPrefab, selectedSocket.transform.position, Quaternion.identity);
        itemSpawned.transform.SetParent(selectedSocket.transform);
    }

    private void DespawnItem()
    {
        //Despawn item
        if (spawnedItem != null)
        {
            Destroy(spawnedItem.gameObject);
            spawnedItem = null;
        }

    }

    private void UpdateSelectedSocket()
    {
        if(selectedItemSO == null)
        {
            Debug.LogWarning("Item not selected");
            return;
        }

        if (isRightSocket)
        {
            foreach (ItemSocket socket in rightSideSockets)
            {
                if (socket.ItemSO == selectedItemSO)
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                }
            }
        } else
        {
            foreach (ItemSocket socket in leftSideSockets)
            {
                if (socket.ItemSO == selectedItemSO)
                {
                    //Found the corresponding socket
                    selectedSocket = socket;
                }
            }
        }


    }
}
