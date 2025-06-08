using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseCameraGlobalFollow : NetworkBehaviour
{ 
    public static event Action OnFinishedFollowing;
    
    private static Transform followTargetTransform;
    private Coroutine followObjectCoroutine;
    private Transform cameraObjectToFollow;
    

    private float cameraZPosOnFollowing = -12f;
    private float followYOffsetForPlayer = 2.5f;
    
    public override void OnNetworkSpawn()
    {
        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
    }

    private void HandleOnItemCallbackAction()
    {
        throw new System.NotImplementedException();
    }

    private void HandleOnItemReleasedAction(Transform itemObject)
    {
        throw new System.NotImplementedException();
    }
    
    /// <summary>
    /// Called to follow a NetworkObject 
    /// </summary>
    /// <param name="objectToFollow"> pass the NetworkObject to follow. MUST BE!</param>
    /// <param name="duration"> duration if want</param>
    /// <param name="followByDuration"> if should follow indefinitely or follow for a short period</param>
    public void FollowObject(NetworkObjectReference objectToFollow, float duration, bool followByDuration)
    {
        FollowObjectServerRpc(objectToFollow, duration, followByDuration);
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void FollowObjectServerRpc(NetworkObjectReference objectToFollow, float duration, bool followByDuration)
    {
        FollowObjectClientRpc(objectToFollow, duration, followByDuration);
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void FollowObjectClientRpc(NetworkObjectReference objectToFollow, float duration, bool followByDuration)
    {
        if (objectToFollow.TryGet(out NetworkObject networkObject))
        {
            followTargetTransform = networkObject.transform;
        }
        else
        {
            Debug.LogWarning("CameraGlobalFollow - FollowObjectClientRpc: NetworkObjectReference is not valid.");
            return;
        }
        
        followObjectCoroutine = StartCoroutine(FollowPositionCoroutine(duration, followByDuration)); 
    }

    public void StopFollowingObject()
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
        StopFollowingCoroutine();
    }
    
    
    private IEnumerator FollowPositionCoroutine(float duration, bool followByDuration = true)    
    {
        float timer = 0f;
        Debug.Log($"Camera - Follow Object: {followTargetTransform.name}");
        if (!followByDuration)
        {
            while (followTargetTransform)
            {
                cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
                yield return null;
            }
            followObjectCoroutine = null;
            OnFinishedFollowing?.Invoke();
        }
        else
        {
            // stop following after a certain duration
            while (timer < duration)
            {
                if (followTargetTransform)
                {
                    cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y + followYOffsetForPlayer, cameraZPosOnFollowing);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObjectCoroutine = null;
            OnFinishedFollowing?.Invoke();
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
