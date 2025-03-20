using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public Action<string> OnClientLeft;
    public Action<UserData> OnUserLeft;
    public Action<UserData> OnUserJoined;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); // save client IDs to their authentication IDs
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); // save authentication IDs to user data

    public NetworkServer(NetworkManager _networkManager, NetworkObject _playerPrefab) // our constructor
    {
        networkManager = _networkManager;
        playerPrefab = _playerPrefab;
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

        if (clientIdToAuth.TryGetValue(clientId, out string authId)) //Handle disconnections
        {
            clientIdToAuth.Remove(clientId);
            OnUserLeft?.Invoke(authIdToUserData[authId]);
            authIdToUserData.Remove(authId);

            OnClientLeft?.Invoke(authId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload); //Deserialize the payload to jason

        UserData userData = JsonUtility.FromJson<UserData>(payload); //Deserialize the payload to UserData

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId; //if dont exist, add to dictionary
        authIdToUserData[userData.userAuthId] = userData;

        OnUserJoined?.Invoke(userData);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        _ = SpawnPlayer(request.ClientNetworkId);
    }

    private async Task SpawnPlayer(ulong clientId)
    {
        await Task.Delay(2000); //delay to wait the client load the scene. Improve this!

        Transform randomSpawnPointSelected = GameFlowManager.Instance.GetRandomSpawnPoint();

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, randomSpawnPointSelected.position, Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);

        Debug.Log($"Connected Clients: {networkManager.ConnectedClientsList.Count}");

        if(networkManager.ConnectedClientsList.Count == 1) // PROB NOT WORKING FOR DEDICATED SERVER
        {
            playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(PlayableState.Player1Playing, randomSpawnPointSelected.rotation);

        } else if (networkManager.ConnectedClientsList.Count == 2)
        {
            playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(PlayableState.Player2Playing, randomSpawnPointSelected.rotation);

            //Both players are connected and spawned
            GameFlowManager.Instance.ChangeGameState(GameState.WaitingToStart);
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
