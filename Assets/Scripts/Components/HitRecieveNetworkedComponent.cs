using System;
using Unity.Netcode;
using UnityEngine;

public class HitRecieveNetworkedComponent : NetworkBehaviour, IRecieveHit
{
    public event Action OnHitRecieve;

    //DEBUG
    public bool hitRecieve = false;

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
        Debug.Log("Getup - HitRecieveComponent");
        hitRecieve = true;
        OnHitRecieve?.Invoke();
    }
}
