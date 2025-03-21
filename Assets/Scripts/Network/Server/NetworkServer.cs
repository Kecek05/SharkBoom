using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{

    public Action<string> OnClientLeft;
    public Action<UserData> OnUserLeft;
    public Action<UserData> OnUserJoined;

    private NetworkManager networkManager;
    private PlayerSpawner playerSpawner;
    private ServerAuthenticationService serverAuthenticationService;


    public PlayerSpawner PlayerSpawner => playerSpawner;
    public ServerAuthenticationService ServerAuthenticationService => serverAuthenticationService;

    public NetworkServer(NetworkManager _networkManager,NetworkObject _playerPrefab) // our constructor
    {
        networkManager = _networkManager;
        playerSpawner = new(_playerPrefab);
        serverAuthenticationService = new();
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        networkManager.OnServerStarted += NetworkManager_OnServerStarted;
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = networkManager.gameObject.GetComponent<UnityTransport>();

        transport.SetConnectionData(ip, (ushort)port);

        return networkManager.StartServer(); //returns a bool if successful
    }

    private void NetworkManager_OnServerStarted()
    {
        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");

        OnUserLeft?.Invoke(serverAuthenticationService.GetUserDataByClientId(clientId));
        OnClientLeft?.Invoke(serverAuthenticationService.GetAuthIdByClientId(clientId));
        serverAuthenticationService.UnregisterClient(clientId);

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload); //Deserialize the payload to jason

        UserData userData = JsonUtility.FromJson<UserData>(payload); //Deserialize the payload to UserData

        serverAuthenticationService.RegisterClient(request.ClientNetworkId, userData);

        OnUserJoined?.Invoke(userData);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        Debug.Log($"Connected Clients: {networkManager.ConnectedClientsList.Count}");

        if(serverAuthenticationService.RegisteredClientCount == 2) //two clients in game
            GameFlowManager.Instance.ChangeGameState(GameState.SpawningPlayers);
        
    }


    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnServerStarted -= NetworkManager_OnServerStarted;
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

        }

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
