using System;
using System.Collections;
using Sortify;
using UnityEngine;

public class PlayerRotateToAim : DragListener, IInitializeOnwer, IDetectDragChange, IDetectEndedTurn
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private float lerpSpeed = 5f;
    private float lerpFinishThreshold = 0.01f;
    private Coroutine aimLerpCoroutine;
    
    public Transform AimTransform => aimTransform;
    
    public void DoOnInitializeOnwer()
    {
        ResetAimPosition();
    }

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        aimTransform.position = playerDragController.GetOpositeFingerPos();
    }
    
    

    public void SyncAimPosition(Vector3 targetPosition, Action onFinishLerpAim = null)
    {
        if (aimLerpCoroutine != null)
            StopCoroutine(aimLerpCoroutine);

        aimLerpCoroutine = StartCoroutine(LerpAimPositionCoroutine(targetPosition, onFinishLerpAim));
    }

    private IEnumerator LerpAimPositionCoroutine(Vector3 targetPosition, Action onFinishLerpAim)
    {
        Debug.Log($"LerpAimPositionCoroutine - Target Position: {targetPosition}, Current Position: {aimTransform.position}");
        while (Vector3.Distance(aimTransform.position, targetPosition) > lerpFinishThreshold)
        {
            aimTransform.position = Vector3.Lerp(aimTransform.position, targetPosition, Time.deltaTime * lerpSpeed);
            Debug.Log($"Lerping aim position to {targetPosition}, current position: {aimTransform.position}");
            yield return null;
        }
        aimTransform.position = targetPosition;
        onFinishLerpAim?.Invoke();
        aimLerpCoroutine = null;
    }
    
    private void ResetAimPosition()
    {
        aimTransform.position = aimDefaultPosition.position;
    }
    
    public void OnRagdollDisabled()
    {
        ResetAimPosition();
    }

    public void DoOnEndedTurn()
    {
        ResetAimPosition();
    }
    
}
