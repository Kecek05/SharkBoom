using Unity.Netcode;
using UnityEngine;

public class ItemActivableManager : BaseItemActivableManager
{

    public override void UseItem()
    {
        //always local machine will call this

        if (itemThrowableActivableClient)
        {
            itemThrowableActivableClient.TryActivate();
            UseItemServerRpc();
        }
        else
        {
            Debug.LogWarning("ItemThrowableActivableClient is null, cannot use item.");
        }
    }

    protected override void UseItemServer()
    {
        UseItemServerRpc();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void UseItemServerRpc()
    {

        if (itemThrowableActivableServer)
            itemThrowableActivableServer.TryActivate();

        //UseItemClientRpc(serverRpc.Receive.SenderClientId);

    }

    protected override void UseItemClient(ulong clientIdUsed)
    {
        UseItemClientRpc(clientIdUsed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UseItemClientRpc(ulong clientIdUsed) //change to other thing rather than clientId
    {
        if (clientIdUsed == NetworkManager.Singleton.LocalClientId) return; //already called on client

        if (itemThrowableActivableClient)
            itemThrowableActivableClient.TryActivate();
    }

    public override void SetItemThrowableActivableClient(BaseItemThrowableActivable itemThrowableActivableClient)
    {
        this.itemThrowableActivableClient = itemThrowableActivableClient;
    }

    public override void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer)
    {
        this.itemThrowableActivableServer = itemThrowableActivableServer;
    }

    public override void ResetItemActivable()
    {
        itemThrowableActivableClient = null;
        itemThrowableActivableServer = null;
    }

}
