using Unity.Netcode;

public abstract class BaseItemActivableManager : NetworkBehaviour
{

    protected BaseItemThrowableActivable itemThrowableActivable;
    protected BaseItemThrowableActivable itemThrowableActivableServer;

    public BaseItemThrowableActivable ItemThrowableActivable => itemThrowableActivable;
    public BaseItemThrowableActivable ItemThrowableActivableServer => itemThrowableActivableServer;


    public abstract void UseItem();
    
    public abstract void ReconcileItem(ItemReconcileData reconcileData);
    //[ServerRpc(RequireOwnership = false)]
    protected abstract void UseItemServer();

    //[Rpc(SendTo.ClientsAndHost)]
    protected abstract void UseItemClient(ulong clientIdUsed); //change to other thing rather than clientId

    public abstract void SetItemThrowableActivable(BaseItemThrowableActivable itemThrowableActivable);

    public abstract void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer);

    public abstract void ResetItemActivable();

    //public abstract void ReconcileItem();
}
