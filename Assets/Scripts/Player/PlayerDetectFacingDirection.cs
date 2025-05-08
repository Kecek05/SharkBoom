using Sortify;
using System;
using System.Collections;
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
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        if(delayStartFaceOtherPlayerCoroutine != null)
        {
            StopCoroutine(delayStartFaceOtherPlayerCoroutine);
            delayStartFaceOtherPlayerCoroutine = null;
        }

        delayStartFaceOtherPlayerCoroutine = StartCoroutine(DelayStartFaceOtherPlayer());
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
        Debug.Log($"DragChange Oposite Finger Pos X: {playerDragController.GetOpositeFingerPos().x} - PlayerGFX Pos X: {playerGfxTransform.position.x} - isRight: {isDirectionRight}");
    }

    private IEnumerator DelayStartFaceOtherPlayer()
    {
        //Wait for the end of the frame before executing the code to ensure that all scripts subscribe to the event OnRotationChanged
        yield return new WaitForEndOfFrame();
        FaceOtherPlayer();
    }

    private void FaceOtherPlayer()
    {
        isDirectionRight = LocateOtherPlayer.OtherPlayerIsOnMyRight(turnManager.LocalPlayableState);

        OnRotationChanged?.Invoke(isDirectionRight);

        Debug.Log("Is Direction Right: " + isDirectionRight);
    }

    public void DoOnEndedTurn()
    {
        FaceOtherPlayer();
    }

}
