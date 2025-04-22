using System;
using UnityEngine;

public class PlayerAnimatorTest : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    [SerializeField] private PlayerLauncherTest playerLauncherTest;

    [SerializeField] private ItemsListSO itemSOList;

    private ItemSO itemSelectedSO;

    private void Start()
    {
        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;
        playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        Debug.Log("PlayerAnimatorTest started");
        //Start with Idle
        SetIsRight(false);
        SetPlayerStateByIndex(0);
    }

    private void HandleOnPlayerAnimatorCrossfadeFinished()
    {
        playerSpawnItemOnHand.HandleOnCrossfadeFinished();
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
        playerSpawnItemOnHand.HandleOnItemSelectedSO(itemSelectedSO);
    }

    private void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        playerAnimator.HandleOnPlayerStateMachineStateChanged(newState);
        playerSpawnItemOnHand.HandleOnPlayerStateChanged(newState);
    }

    private void HandleOnRotationChanged(bool isRight)
    {
        playerAnimator.HandleOnRotationChanged(isRight);
        playerSpawnItemOnHand.HandleOnRotationChanged(isRight);
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
