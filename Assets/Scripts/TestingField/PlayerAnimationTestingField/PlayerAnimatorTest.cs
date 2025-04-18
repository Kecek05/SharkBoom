using UnityEngine;

public class PlayerAnimatorTest : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;

    [SerializeField] private ItemsListSO itemSOList;

    private ItemSO itemSelectedSO;

    private void Start()
    {
        //Start with Idle
        SetPlayerStateByIndex(0);
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
    }

    private void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        playerAnimator.HandleOnPlayerStateMachineStateChanged(newState);
    }

    private void HandleOnRotationChanged(bool isRight)
    {
        playerAnimator.HandleOnRotationChanged(isRight);
    }
}
