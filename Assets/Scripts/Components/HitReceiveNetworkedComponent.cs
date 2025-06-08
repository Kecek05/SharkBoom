using System;
using Unity.Netcode;
using UnityEngine;

public class HitReceiveNetworkedComponent : NetworkBehaviour, IRecieveHit
{
    public event Action OnHitReceive;

    //DEBUG
    //public bool hitRecieve = false;

    public void Hit()
    {
        HitServerRpc();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void HitServerRpc()
    {
        HitClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void HitClientRpc()
    {
        Debug.Log("Getup - HitReceiveComponent");
        OnHitReceive?.Invoke();
    }
}
