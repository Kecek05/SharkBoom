using System;
using Unity.Netcode;
using UnityEngine;

public class HitReceiveNetworkedComponent : NetworkBehaviour, IRecieveHit
{
    /// <summary>
    /// Called when the object receives a hit. Pass if is jump to camera not follow.
    /// </summary>
    public event Action<bool> OnHitReceive;

    //DEBUG
    //public bool hitRecieve = false;

    public void Hit(bool isJump)
    {
        HitServerRpc(isJump);
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void HitServerRpc(bool isJump)
    {
        HitClientRpc(isJump);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void HitClientRpc(bool isJump)
    {
        OnHitReceive?.Invoke(isJump);
    }
}
