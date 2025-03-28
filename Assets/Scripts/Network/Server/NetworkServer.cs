using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{

    public Action<string> OnClientLeft;
    public Action<PlayerData> OnUserLeft;
    public Action<PlayerData> OnUserJoined;

    private NetworkManager networkManager;
    private IPlayerSpawner playerSpawner;
    private IServerAuthenticationService serverAuthenticationService;


    public IPlayerSpawner PlayerSpawner => playerSpawner;
    public IServerAuthenticationService ServerAuthenticationService => serverAuthenticationService;

    public NetworkServer(NetworkManager _networkManager,NetworkObject _playerPrefab) // our constructor
    {
        networkManager = _networkManager;
        playerSpawner = new PlayerSpawner(_playerPrefab);
        serverAuthenticationService = new ServerAuthenticationService();
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        networkManager.OnServerStarted += NetworkManager_OnServerStarted;

    }


    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        //Code to spawn players

        if(sceneName != Loader.Scene.GameNetCodeTest.ToString()) return; //Only Spawn players in Game Scene


        Debug.Log($"Client {clientId} / AuthId {serverAuthenticationService.GetAuthIdByClientId(clientId)} loaded scene {sceneName}");

        playerSpawner.SpawnPlayer(clientId);
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

        networkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");

        OnUserLeft?.Invoke(serverAuthenticationService.GetPlayerDataByClientId(clientId));
        OnClientLeft?.Invoke(serverAuthenticationService.GetAuthIdByClientId(clientId));
        serverAuthenticationService.UnregisterClient(clientId);

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload); //Deserialize the payload to jason

        UserData userData = JsonUtility.FromJson<UserData>(payload); //Deserialize the payload to UserData

        PlayerData newPlayerData = new PlayerData()
        {
            userData = userData,
            clientId = request.ClientNetworkId,
            playableState = PlayableState.None, //None for now
            calculatedPearls = new CalculatedPearls(),
            gameObject = null
        };

        serverAuthenticationService.RegisterClient(newPlayerData);



        OnUserJoined?.Invoke(newPlayerData);

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        if(serverAuthenticationService.RegisteredClientCount == 2) //two clients in game
            GameFlowManager.Instance.GameStateManager.ChangeGameState(GameState.SpawningPlayers);
        
    }


    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnServerStarted -= NetworkManager_OnServerStarted;
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

            if(networkManager.SceneManager != null)
                networkManager.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
        }

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
