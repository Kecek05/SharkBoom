using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraGlobalFollow : NetworkBehaviour
{
    
    private static Transform followTargetTransform;
    private Coroutine followObjectCoroutine;
    private Transform cameraObjectToFollow;

    private float cameraZPosOnFollowing = -30f;
    [SerializeField] private float cameraFollowSpeed = 12f;
    
    public override void OnNetworkSpawn()
    {
        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

    public void Initialize(Transform _cameraObjectToFollow)
    {
        cameraObjectToFollow = _cameraObjectToFollow;
    }

    private void HandleOnItemCallbackAction()
    {
        StopFollowingCoroutine();
    }

    private void HandleOnItemReleasedAction(Transform itemObject)
    {
        FollowObject(itemObject); //TO DO: FIX THIS
    }

    /// <summary>
    /// Called to follow an object in the scene.
    /// </summary>
    /// <param name="objectToFollow"> Transform of the Object to Follow</param>
    /// <param name="duration"> Duration of the Follow</param>
    /// <param name="followByDuration"> If Should Follow by duration | If false, will follow until some callback to stop it</param>
    /// <param name="onComplete"> Callback on complete following</param>
    /// <param name="isJump"> True if is jump, jump shouldnt follow the player hited by the jump</param>
    public void FollowObject(Transform objectToFollow, float duration = 0, bool followByDuration = false, Action onComplete = null, bool isJump = false, bool isInterpolate = true)
    {
        if(isJump) return;
        
        followTargetTransform = objectToFollow;
        
        StopFollowingCoroutine();
        followObjectCoroutine = StartCoroutine(FollowPositionCoroutine(duration, followByDuration, onComplete, isInterpolate)); 
    }
    private IEnumerator FollowPositionCoroutine(float duration, bool followByDuration, Action onComplete, bool isInterpolate)    
    {
        float timer = 0f;
        if (!followByDuration)
        {
            while (followTargetTransform)
            {
                ChangeCameraObjectToFollowPosition(isInterpolate);
                yield return null;
            }
            followObjectCoroutine = null;
            onComplete?.Invoke();
        }
        else
        {
            // stop following after a certain duration
            while (timer < duration)
            {
                if (followTargetTransform)
                {
                    ChangeCameraObjectToFollowPosition(isInterpolate);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObjectCoroutine = null;
            onComplete?.Invoke();
        }
    }

    private void ChangeCameraObjectToFollowPosition(bool isInterpolate)
    {
        if (isInterpolate)
        {
            Vector3 targetPos = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
            cameraObjectToFollow.position = Vector3.Lerp(cameraObjectToFollow.position, targetPos, cameraFollowSpeed * Time.deltaTime);
        }
        else
        {
            cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
        }
    }

    private void StopFollowingCoroutine()
    {
        if (followObjectCoroutine != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObjectCoroutine);
        }

        followObjectCoroutine = null;
    }
    
    public override void OnNetworkDespawn()
    {
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;
    }
}
