using Unity.Netcode;
using UnityEngine;

public class ItemActivableManager : NetworkBehaviour
{
    private static ItemActivableManager instance;
    public static ItemActivableManager Instance => instance;

    private BaseItemThrowableActivable itemThrowableActivableClient;
    private BaseItemThrowableActivable itemThrowableActivableServer;


    public BaseItemThrowableActivable ItemThrowableActivableClient => itemThrowableActivableClient;
    public BaseItemThrowableActivable ItemThrowableActivableServer => itemThrowableActivableServer;

    private void Awake()
    {
        instance = this;
    }

    public void UseItem()
    {
        //always local machine will call this

        if (itemThrowableActivableClient != null)
            itemThrowableActivableClient.TryActivate();

        UseItemServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseItemServerRpc(ServerRpcParams serverRpc = default)
    {

        if (itemThrowableActivableServer != null)
            itemThrowableActivableServer.TryActivate();

        UseItemClientRpc(serverRpc.Receive.SenderClientId);

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UseItemClientRpc(ulong clientIdUsed) //change to other thing rather than clientId
    {
        if (clientIdUsed == NetworkManager.Singleton.LocalClientId) return; //already called on client

        if (itemThrowableActivableClient != null)
            itemThrowableActivableClient.TryActivate();
    }

    public void SetItemThrowableActivableClient(BaseItemThrowableActivable itemThrowableActivableClient)
    {
        this.itemThrowableActivableClient = itemThrowableActivableClient;
    }

    public void SetItemThrowableActivableServer(BaseItemThrowableActivable itemThrowableActivableServer)
    {
        this.itemThrowableActivableServer = itemThrowableActivableServer;
    }

    public void ResetItemActivable()
    {
        itemThrowableActivableClient = null;
        itemThrowableActivableServer = null;
    }
}
