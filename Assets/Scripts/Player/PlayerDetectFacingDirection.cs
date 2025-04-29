using Sortify;
using System;
using UnityEngine;

public class PlayerDetectFacingDirection : DragListener, IInitializeOnwer, IDetectDragChange, IDetectEndedTurn
{
    /// <summary>
    /// Called when the look orientation is changed. Pass if is looking right
    /// </summary>
    public event Action<bool> OnRotationChanged;


    [BetterHeader("References")]
    [SerializeField] private Transform playerGfxTransform;
    [SerializeField] private PlayerDragController playerDragController;
    [Tooltip("Value to be add to not rotate the object to close to the 90 degrees")]
    [SerializeField] private float angleOffset = 0.5f;

    private BaseTurnManager turnManager;
    private bool isDirectionRight = false;

    public void DoOnInitializeOnwer()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
    }

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        if (playerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x + angleOffset)
        {
            //right
            if (isDirectionRight) return; //do nothing if the direction is already right

            isDirectionRight = true;

            OnRotationChanged?.Invoke(true);

            Debug.Log("Right");

        }
        else if (playerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
        {
            //left
            if (!isDirectionRight) return; //do nothing if the direction is already left

            isDirectionRight = false;

            OnRotationChanged?.Invoke(false);

            Debug.Log("Left");
        }
    }

    public void DoOnEndedTurn()
    {
        isDirectionRight = LocateOtherPlayer.OtherPlayerIsOnMyRight(turnManager.LocalPlayableState);

        OnRotationChanged?.Invoke(isDirectionRight);
    }

}
