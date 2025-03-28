using Unity.Netcode;

public abstract class BaseItemActivableManager : NetworkBehaviour
{

    protected BaseItemThrowableActivable itemThrowableActivableClient;
    protected BaseItemThrowableActivable itemThrowableActivableServer;

    public BaseItemThrowableActivable ItemThrowableActivableClient => itemThrowableActivableClient;
    public BaseItemThrowableActivable ItemThrowableActivableServer => itemThrowableActivableServer;


    public abstract void UseItem();

    //[ServerRpc(RequireOwnership = false)]
    protected abstract void UseItemServer(ServerRpcParams serverRpc = default);

    //[Rpc(SendTo.ClientsAndHost)]
    protected abstract void UseItemClient(ulong clientIdUsed); //change to other thing rather than clientId

    public abstract void SetItemThrowableActivableClient(BaseItemThrowableActivable itemThrowableActivableClient);

    public abstract void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer);

    public abstract void ResetItemActivable();
}
