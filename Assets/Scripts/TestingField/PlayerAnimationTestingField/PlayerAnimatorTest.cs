using UnityEngine;

public class PlayerAnimatorTest : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;

    [SerializeField] private ItemsListSO itemSOList;

    private ItemSO itemSelectedSO;

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

    public void SetPlayerState(PlayerState playerState)
    {
        HandleOnPlayerStateMachineStateChanged(playerState);
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
