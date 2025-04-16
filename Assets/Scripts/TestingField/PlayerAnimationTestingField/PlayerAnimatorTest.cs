using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimatorTest : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnimator;

    //[SerializeField] private List<ItemSO> itemSOList;

   // [SerializeField] private Button 


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
