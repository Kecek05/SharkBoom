using Unity.Netcode;

public class ItemActivableManager : BaseItemActivableManager
{

    public override void UseItem()
    {
        //always local machine will call this

        if (itemThrowableActivableClient != null)
            itemThrowableActivableClient.TryActivate();

        UseItemServerRpc();
    }

    protected override void UseItemServer(ServerRpcParams serverRpc = default)
    {
        UseItemServerRpc(serverRpc);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseItemServerRpc(ServerRpcParams serverRpc = default)
    {

        if (itemThrowableActivableServer != null)
            itemThrowableActivableServer.TryActivate();

        UseItemClientRpc(serverRpc.Receive.SenderClientId);

    }

    protected override void UseItemClient(ulong clientIdUsed)
    {
        UseItemClientRpc(clientIdUsed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UseItemClientRpc(ulong clientIdUsed) //change to other thing rather than clientId
    {
        if (clientIdUsed == NetworkManager.Singleton.LocalClientId) return; //already called on client

        if (itemThrowableActivableClient != null)
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
