using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
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

    private Coroutine delayStartFaceOtherPlayerCoroutine;

    public void DoOnInitializeOnwer()
    {
        SetupDetectFacingDirection();

        delayStartFaceOtherPlayerCoroutine = StartCoroutine(DelayStartFaceOtherPlayer());
    }

    public void SetupDetectFacingDirection()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        
        if(delayStartFaceOtherPlayerCoroutine != null)
        {
            StopCoroutine(delayStartFaceOtherPlayerCoroutine);
            delayStartFaceOtherPlayerCoroutine = null;
        }
    }
    
    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        if (playerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x + angleOffset)
        {
            //right
            if (isDirectionRight) return; //do nothing if the direction is already right

            isDirectionRight = true;

            OnRotationChanged?.Invoke(true);
        }
        else if (playerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
        {
            //left
            if (!isDirectionRight) return; //do nothing if the direction is already left

            isDirectionRight = false;

            OnRotationChanged?.Invoke(false);
        }
    }

    private IEnumerator DelayStartFaceOtherPlayer()
    {
        //Wait for the end of the frame before executing the code to ensure that all scripts subscribe to the event OnRotationChanged
        yield return new WaitForEndOfFrame();
        FaceOtherPlayer();
    }

    public void FaceOtherPlayer()
    {
        if(!IsOwner) return;
        isDirectionRight = LocateOtherPlayer.OtherPlayerIsOnMyRight(turnManager.LocalPlayableState);

        OnRotationChanged?.Invoke(isDirectionRight);

        RotationChangedServerRpc(isDirectionRight);
    }

    [Rpc(SendTo.Server)]
    private void RotationChangedServerRpc(bool isRight)
    {
        RotationChangedClientRpc(isRight);
    }
    
    [Rpc(SendTo.NotOwner)]
    private void RotationChangedClientRpc(bool isRight)
    {
        OnRotationChanged?.Invoke(isDirectionRight);
    }

    public void DoOnEndedTurn()
    {
        FaceOtherPlayer();
    }
}
