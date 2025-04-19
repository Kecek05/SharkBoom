using Sortify;
using UnityEngine;

public class PlayerSpawnItemOnHand : MonoBehaviour
{

    [SerializeField] private ItemSocket[] leftSideSockets;
    [SerializeField] private ItemSocket[] rightSideSockets;

    private ItemSocket selectedSocket;
    private ItemSO selectedItemSO;
    private bool isRightSocket = false; //Rotation that the player is looking
    private GameObject spawnedItem;
    private PlayerState playerState;
    private bool canSpawnItem = false;

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

    public void HandleOnPlayerAnimatorCrossfadeFinished()
    {
        Debug.Log("HandleOnPlayerAnimatorCrossfadeFinished");
        SpawnItem();
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        Debug.Log($"HandleOnPlayerStateMachineStateChanged: {newState}");

        playerState = newState;
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
        Debug.Log($"Spawning item: {selectedItemSO} on socket: {selectedSocket} - PlayerState: {playerState} - Can Spawn? {canSpawnItem}");
        if (!canSpawnItem) return; //Do nothing if the player is not in the right state

        if (selectedItemSO == null)
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

        spawnedItem = Instantiate(selectedItemSO.itemClientPrefab, selectedSocket.transform.position, Quaternion.identity);
        spawnedItem.transform.SetParent(selectedSocket.transform);
        spawnedItem.transform.localRotation = Quaternion.identity;

        spawnedItem.GetComponent<BaseItemThrowable>().Initialize(PlayableState.None);
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
