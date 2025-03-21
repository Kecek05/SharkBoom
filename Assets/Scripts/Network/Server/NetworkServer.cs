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

    private Dictionary<string, ulong> authToClientId = new Dictionary<string, ulong>(); // save authentication IDs to their client IDs
    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>(); // save client IDs to their authentication IDs
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>(); // save authentication IDs to user data

    public Dictionary<ulong, string> ClientIdToAuth => clientIdToAuth;
    public Dictionary<string, ulong> AuthToClientId => authToClientId;

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
            authToClientId.Remove(authId);
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
        authToClientId[userData.userAuthId] = request.ClientNetworkId; //if dont exist, add to dictionary
        authIdToUserData[userData.userAuthId] = userData;


        OnUserJoined?.Invoke(userData);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        Debug.Log($"Connected Clients: {networkManager.ConnectedClientsList.Count}");

        if(clientIdToAuth.Count == 2) //two clients in game
            GameFlowManager.Instance.ChangeGameState(GameState.SpawningPlayers);
        
    }

    /// <summary>
    /// Call this to spawn a player.
    /// </summary>
    /// <param name="clientId"> Id of the player</param>
    /// <param name="playerNumber"> player playing state</param>
    public void SpawnPlayer(ulong clientId, PlayableState playerPlayingState)
    {

        Transform randomSpawnPointSelected = GameFlowManager.Instance.GetRandomSpawnPoint();

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, randomSpawnPointSelected.position, Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);

        playerInstance.GetComponent<PlayerThrower>().InitializePlayerRpc(playerPlayingState, randomSpawnPointSelected.rotation);

        Debug.Log($"Spawning Player, Client Id: {clientId} PlayableState: {playerPlayingState} Selected Random SpawnPoint: {randomSpawnPointSelected.name}");
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
