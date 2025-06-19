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
    [SerializeField] private PlayerThrower playerThrower;
    
    private BaseTurnManager turnManager;
    private bool isDirectionRight = false;

    private Coroutine delayStartFaceOtherPlayerCoroutine;
    
    //DEBUG
    public bool IsDirectionRight => isDirectionRight;

    public void DoOnInitializeOnwer()
    {
        SetupDetectFacingDirection();

        delayStartFaceOtherPlayerCoroutine = StartCoroutine(DelayStartFaceOtherPlayer());
    }

    public void SetupDetectFacingDirection()
    {
       // turnManager = ServiceLocator.Get<BaseTurnManager>();
        
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
            
            // RotationChangedServerRpc(true);
        }
        else if (playerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
        {
            //left
            if (!isDirectionRight) return; //do nothing if the direction is already left

            isDirectionRight = false;

            OnRotationChanged?.Invoke(false);
            
            // RotationChangedServerRpc(false);
        }
    }

    private IEnumerator DelayStartFaceOtherPlayer()
    {
        //Wait for the end of the frame before executing the code to ensure that all scripts subscribe to the event OnRotationChanged
        yield return new WaitForSeconds(0.2f);
        FaceOtherPlayer();
    }

    public void FaceOtherPlayer()
    {
       // if(!IsOwner) return;
        isDirectionRight = LocateOtherPlayer.OtherPlayerIsOnMyRight(playerThrower.ThisPlayableState.Value);

        OnRotationChanged?.Invoke(isDirectionRight);
    }

    // [Rpc(SendTo.Server)]
    // private void RotationChangedServerRpc(bool isRight)
    // {
    //     RotationChangedClientRpc(isRight);
    // }
    
    public void SetRotation(bool isRight)
    {
        isDirectionRight = isRight;
        
        OnRotationChanged?.Invoke(isRight);
    }

    public void DoOnEndedTurn()
    {
        FaceOtherPlayer();
    }
}
