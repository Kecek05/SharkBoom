using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public NetworkServer(NetworkManager _networkManager, NetworkObject _playerPrefab) // our constructor
    {
        networkManager = _networkManager;
        playerPrefab = _playerPrefab;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        networkManager.OnServerStarted += NetworkManager_OnServerStarted;
    }

    private void NetworkManager_OnServerStarted()
    {
        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {

        _ = SpawnPlayerDelay(request.ClientNetworkId);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        if(networkManager.ConnectedClientsList.Count == 1) 
        {
            //Both players are connected
            GameFlowManager.Instance.SetGameStateRpc(GameFlowManager.GameState.GameStarted);
        }
    }

    private async Task SpawnPlayerDelay(ulong clientId)
    {
        await Task.Delay(1000);
        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, GameFlowManager.Instance.GetRandomSpawnPoint(), Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);
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
