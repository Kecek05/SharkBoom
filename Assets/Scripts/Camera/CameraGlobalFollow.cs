using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraGlobalFollow : NetworkBehaviour
{
    private Action OnComplete;
    
    private static Transform followTargetTransform;
    private Coroutine followObjectCoroutine;
    private Transform cameraObjectToFollow;
    

    private float cameraZPosOnFollowing = -30f;
    //[SerializeField] private float cameraFollowSpeed = 10f;
    
    public override void OnNetworkSpawn()
    {
        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
        Debug.Log("CameraGlobalFollow - Subscribed to CameraFollowing events");
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
        FollowObject(itemObject);
    }

    /// <summary>
    /// Called to follow an object in the scene.
    /// </summary>
    /// <param name="objectToFollow"> Transform of the Object to Follow</param>
    /// <param name="duration"> Duration of the Follow</param>
    /// <param name="followByDuration"> If Should Follow by duration | If false, will follow until some callback to stop it</param>
    /// <param name="onComplete"> Callback on complete following</param>
    /// <param name="isJump"> True if is jump, jump shouldnt follow the player hited by the jump</param>
    public void FollowObject(Transform objectToFollow, float duration = 0, bool followByDuration = false, Action onComplete = null, bool isJump = false)
    {
        if(isJump) return;
        this.OnComplete = onComplete;

        followTargetTransform = objectToFollow;
        
        StopFollowingCoroutine();
        followObjectCoroutine = StartCoroutine(FollowPositionCoroutine(duration, followByDuration)); 
    }

    // [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    // private void FollowObjectServerRpc(NetworkObjectReference objectToFollow, float duration, bool followByDuration)
    // {
    //     FollowObjectClientRpc(objectToFollow, duration, followByDuration);
    // }
    //
    // [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    // private void FollowObjectClientRpc(NetworkObjectReference objectToFollow, float duration, bool followByDuration)
    // {
    //     if (objectToFollow.TryGet(out NetworkObject networkObject))
    //     {
    //         if (networkObject.transform.TryGetComponent(out PlayerThrower playerThrower))
    //         {
    //             //If is player, follow hips transform
    //             followTargetTransform = playerThrower.HipsTransform;
    //             Debug.Log($"CameraGlobalFollow - FollowObject called with object: {playerThrower.gameObject.name}");
    //             
    //         }
    //         else
    //         {
    //             followTargetTransform = networkObject.transform;
    //             Debug.Log($"CameraGlobalFollow - FollowObject called with object: {followTargetTransform.name}");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("CameraGlobalFollow - FollowObjectClientRpc: NetworkObjectReference is not valid.");
    //         return;
    //     }
    //     
    //     StopFollowingCoroutine();
    //     followObjectCoroutine = StartCoroutine(FollowPositionCoroutine(duration, followByDuration)); 
    // }

    /*public void StopFollowingObject()
    {
        StopFollowingObjectServerRpc();
    }
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void StopFollowingObjectServerRpc()
    {
        StopFollowingObjectClientRpc();
    }
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void StopFollowingObjectClientRpc()
    {

    }*/
    
    
    private IEnumerator FollowPositionCoroutine(float duration, bool followByDuration)    
    {
        float timer = 0f;
        if (!followByDuration)
        {
            while (followTargetTransform)
            {
                ChangeCameraObjectToFollowPosition();
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
                    ChangeCameraObjectToFollowPosition();
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObjectCoroutine = null;
            OnComplete?.Invoke();
        }
    }

    private void ChangeCameraObjectToFollowPosition()
    {
        cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
        
        /*Vector3 targetPos = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
        cameraObjectToFollow.position = Vector3.Lerp(cameraObjectToFollow.position, targetPos, cameraFollowSpeed * Time.deltaTime);*/
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
