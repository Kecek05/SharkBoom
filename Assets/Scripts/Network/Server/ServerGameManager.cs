using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ServerGameManager : IDisposable
{

    private string serverIP;
    private int serverPort;
    private int serverQPort;

    private NetworkServer networkServer;
    private MultiplayAllocationService multiplayAllocationService;


    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = queryPort;

        networkServer = new NetworkServer(networkManager, playerPrefab);

        multiplayAllocationService = new MultiplayAllocationService();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck(); //health check

        if(!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("NetworkServer did not start as expected.");
            return;
        }

        Loader.LoadNetwork(Loader.Scene.GameNetCodeTest);
    }

    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }


}
