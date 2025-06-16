using Unity.Netcode;
using UnityEngine;

public class ItemActivableManager : BaseItemActivableManager
{

    public override void UseItem()
    {
        //always local machine will call this

        if (itemThrowableActivable)
        {
            itemThrowableActivable.TryActivate();
            //UseItemServerRpc();
        }
        else
        {
            Debug.LogWarning("ItemThrowableActivableClient is null, cannot use item.");
        }
    }
    
    public override void ReconcileItem(ItemReconcileData reconcileData)
    {
        Debug.Log("RECONCILE - ReconcileItemClientRpc called");

        if (itemThrowableActivable)
        {
            itemThrowableActivable.Reconcile(reconcileData);
        }
        else
        {
            Debug.LogWarning("ItemThrowableActivableClient is null when trying to Reconcile it! THIS IS AN ERROR!");
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

        if (itemThrowableActivable)
            itemThrowableActivable.TryActivate();
    }

    public override void SetItemThrowableActivable(BaseItemThrowableActivable itemThrowableActivable)
    {
        this.itemThrowableActivable = itemThrowableActivable;
    }

    public override void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer)
    {
        this.itemThrowableActivableServer = itemThrowableActivableServer;
    }

    public override void ResetItemActivable()
    {
        itemThrowableActivable = null;
        itemThrowableActivableServer = null;
    }

}
