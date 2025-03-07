using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); // save client IDs to their authentication IDs
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); // save authentication IDs to user data

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

        OnClientLeft?.Invoke(clientId.ToString());
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload); //Deserialize the payload to jason

        UserData userData = JsonUtility.FromJson<UserData>(payload); //Deserialize the payload to UserData

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId; //if dont exist, add to dictionary
        authIdToUserData[userData.userAuthId] = userData;


        _ = SpawnPlayerDelay(request.ClientNetworkId);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        //if(networkManager.ConnectedClientsList.Count == 1) 
        //{
        //    //Both players are connected
        //    GameFlowManager.Instance.SetGameStateRpc(GameFlowManager.GameState.GameStarted);
        //}
    }

    private async Task SpawnPlayerDelay(ulong clientId)
    {
        await Task.Delay(1000);
        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, GameFlowManager.Instance.GetRandomSpawnPoint(), Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);

        if (networkManager.ConnectedClientsList.Count == 2)
        {
            //Both players are connected and spawned
            GameFlowManager.Instance.SetGameStateRpc(GameFlowManager.GameState.GameStarted);
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if(clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            //Get Auth by client ID
            if (authIdToUserData.TryGetValue(authId, out UserData userData))
            {
                return userData;
            }
        }
        return null;
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
