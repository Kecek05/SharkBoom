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

    public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager networkManager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = serverQPort;

        networkServer = new NetworkServer(networkManager);
    }

    public async Task StartGameServerAsync()
    {
        
    }

    public void Dispose()
    {
        
    }


}
