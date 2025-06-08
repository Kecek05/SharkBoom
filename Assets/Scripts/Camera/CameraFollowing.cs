
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    /// <summary>
    /// Used to say to the other CameraFollowing in the other player to follow the item that was hit.
    /// </summary>
    private static event Action<Transform> OnItemHit;
    /// <summary>
    /// Used to say to the other CameraFollowing in the other player to stop following the item that was hit.
    /// </summary>
    public static event Action OnItemHitCallback;
    
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
        OnItemHit += ItemHitted;
        OnItemHitCallback += ItemCallback;
        if(!IsOwner) return;

        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
    }

    public void HandleOnPlayerHit()
    {
        /*Debug.Log($"HandleOnPlayerHit - This Player: {gameObject.transform.parent.name}");
        SetTarget(hipsBone, true);*/
        OnItemHit?.Invoke(hipsBone.transform);
    }

    public void HandleOnItemCallbackAction()
    {
        OnItemHitCallback?.Invoke();
    }

    private void ItemCallback()
    {
        StopFollowing();
    }

    private void ItemHitted(Transform followTarget)
    {
        if (!IsOwner) return;

        Debug.Log($"HandleOnPlayerHit - This Player: {gameObject.transform.parent.name}");
        SetTarget(followTarget, true);
    }
    private void HandleOnItemReleasedAction(Transform itemLaunched)
    {
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position; // store current position of the camera before the item is launched
        SetTarget(itemLaunched, true);
    }


    public void SetTarget(Transform target, bool followUntilIsNull, float duration = 5f, Action onComplete = null)
    {
        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        followTargetTransform = target;
        this.OnComplete = onComplete;
        StopFollowing();
        
        followObjectCoroutine = StartCoroutine(FollowObjectCoroutine(duration, followUntilIsNull)); 
    }


    private IEnumerator FollowObjectCoroutine(float duration, bool followUntilIsNull = true)    
    {
        float timer = 0f;
        Debug.Log($"Follow Object: {followTargetTransform.name}");
        if (followUntilIsNull)
        {
            while (followTargetTransform)
            {
                cameraManager.CameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
                yield return null;
            }
            followObjectCoroutine = null;
            OnComplete?.Invoke();
        }
        else
        {
            // stop following after a certain duration
            while (timer < duration)
            {
                if (followTargetTransform)
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

    private void StopFollowing()
    {
        if (followObjectCoroutine != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObjectCoroutine);
        }
    }


    public void UnInitializeOwner()
    {
        OnItemHit -= ItemHitted;
        OnItemHitCallback -= ItemCallback;
        if (!IsOwner) return;
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
    }

}
