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
    private float followYOffsetForPlayer = 2.5f;
    
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
        FollowObject(itemObject.GetComponent<NetworkObject>());
    }
    
    /// <summary>
    /// Called to follow a NetworkObject 
    /// </summary>
    /// <param name="objectToFollow"> pass the NetworkObject to follow. MUST BE!</param>
    /// <param name="duration"> duration if want</param>
    /// <param name="followByDuration"> if should follow indefinitely or follow for a short period</param>
    public void FollowObject(NetworkObjectReference objectToFollow, float duration = 0, bool followByDuration = false, Action onComplete = null)
    {
        this.OnComplete = onComplete;
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
            if (networkObject.transform.TryGetComponent(out PlayerThrower playerThrower))
            {
                //If is player, follow hips transform
                followTargetTransform = playerThrower.HipsTransform;
            }
            else
            {
                followTargetTransform = networkObject.transform;
            }
        }
        else
        {
            Debug.LogWarning("CameraGlobalFollow - FollowObjectClientRpc: NetworkObjectReference is not valid.");
            return;
        }

        StopFollowingCoroutine();
        followObjectCoroutine = StartCoroutine(FollowPositionCoroutine(duration, followByDuration)); 
    }

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
                cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y, cameraZPosOnFollowing);
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
                    cameraObjectToFollow.position = new Vector3(followTargetTransform.position.x, followTargetTransform.position.y + followYOffsetForPlayer, cameraZPosOnFollowing);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObjectCoroutine = null;
            OnComplete?.Invoke();
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
