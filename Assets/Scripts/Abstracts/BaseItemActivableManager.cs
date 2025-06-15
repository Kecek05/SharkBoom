using Unity.Netcode;

public abstract class BaseItemActivableManager : NetworkBehaviour
{

    protected BaseItemThrowableActivable itemThrowableActivable;
    protected BaseItemThrowableActivable itemThrowableActivableServer;

    public BaseItemThrowableActivable ItemThrowableActivable => itemThrowableActivable;
    public BaseItemThrowableActivable ItemThrowableActivableServer => itemThrowableActivableServer;


    public abstract void UseItem();

    //[ServerRpc(RequireOwnership = false)]
    protected abstract void UseItemServer();

    //[Rpc(SendTo.ClientsAndHost)]
    protected abstract void UseItemClient(ulong clientIdUsed); //change to other thing rather than clientId

    public abstract void SetItemThrowableActivable(BaseItemThrowableActivable itemThrowableActivable);

    public abstract void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer);

    public abstract void ResetItemActivable();
}
