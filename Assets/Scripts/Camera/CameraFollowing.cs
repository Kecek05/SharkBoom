
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{

    private Action OnComplete;

    [Header("References")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private Transform hipsBone;

    [Header("Settings")]
    [SerializeField] private float cameraZPosOnFollowing = -12f;
    [SerializeField] private float followYOffsetForPlayer = 2.5f;
    private WaitForSeconds waitTimeToStopFollowing = new(3f);
    private Transform followTargetTransform;
    private Vector3 lastCameraObjectToFollowPos;

    private Coroutine followObjectCoroutine;
    private Coroutine resetCameraCoroutine;



    //DEBUG
    public Transform FollowTargetTransformDebug => followTargetTransform;
    
    public void InitializeOwner()
    {
        if(!IsOwner) return;

        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
    }

    private void HandleOnOnItemCallbackAction()
    {
        throw new NotImplementedException();
    }

    public void HandleOnPlayerHit()
    {
        SetTarget(hipsBone, false);
    }

    /*public void HandleOnItemCallbackAction()
    {
        
    }*/

    private void HandleOnItemReleasedAction(Transform itemLaunched)
    {
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position; // store current position of the camera before the item is launched
        SetTarget(itemLaunched, true);
    }


    public void SetTarget(Transform target, bool stopOnNull, float duration = 5f, Action onComplete = null)
    {
        if (!target) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        followTargetTransform = target;
        this.OnComplete = onComplete;

        if (followObjectCoroutine != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObjectCoroutine);
        }

        followObjectCoroutine = StartCoroutine(FollowObjectCoroutine(duration, stopOnNull)); 
    }


    private IEnumerator FollowObjectCoroutine(float duration, bool stopOnNull = true)    
    {
        float timer = 0f;

        if (stopOnNull)
        {
            // used for items that will be destroyed
            while (followTargetTransform != null) // while the itemLaunched is not destroyed
            {
                cameraManager.CameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
                yield return null;
            }
            followObjectCoroutine = null;
            OnComplete?.Invoke();
        }
        else
        {
            // used for player or other item that will not be destroyed
            while (timer < duration)
            {
                if (!followTargetTransform)
                {
                    cameraManager.CameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y + followYOffsetForPlayer, cameraZPosOnFollowing);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObjectCoroutine = null;
            OnComplete?.Invoke();
        }
    }


    public void UnInitializeOwner()
    {
        if (!IsOwner) return;
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
    }

}
