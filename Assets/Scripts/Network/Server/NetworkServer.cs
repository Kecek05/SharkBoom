using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{

    public Action<string> OnClientLeft;
    public Action<PlayerData> OnUserLeft;
    public Action OnUserJoined;

    private NetworkManager networkManager;
    private IPlayerSpawner playerSpawner;
    private IServerAuthenticationService serverAuthenticationService;

    private bool alreadyChangedToSpawningPlayers = false;

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
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload); //Deserialize the payload to jason

        UserData userData = JsonUtility.FromJson<UserData>(payload); //Deserialize the payload to UserData

        Debug.Log($"ApprovalCheck, UserData: {userData.userName}, Pearls: {userData.userPearls}, AuthId: {userData.userAuthId} ");

        CheckReconnect(userData, request.ClientNetworkId);

        OnUserJoined?.Invoke();

        response.Approved = true; //Connection is approved
        response.CreatePlayerObject = false;

        if(serverAuthenticationService.RegisteredClientCount == 2 && !alreadyChangedToSpawningPlayers)
        {
            //two clients in game and not changed to spawning players yet
            alreadyChangedToSpawningPlayers = true;
            ServiceLocator.Get<BaseGameStateManager>().ChangeGameState(GameState.SpawningPlayers);
        }
        
        
    }

    private void CheckReconnect(UserData userData, ulong clientId)
    {
        
        if(playerSpawner.PlayerCount >= 2)
        {
            //already spawned 2 players, this one is reconnecting
            Debug.Log("CheckReconnect, Reconnecting client already spawned");

            //Change Ownership
            
            if(ServerAuthenticationService.AuthIdToPlayerData.TryGetValue(userData.userAuthId, out PlayerData playerData))
            {
                //Update clientId
                playerData = ServerAuthenticationService.AuthIdToPlayerData[userData.userAuthId];

                playerData.clientId = clientId;

                //Change Onwership
                ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState).GetComponent<NetworkObject>().ChangeOwnership(clientId);

                Debug.Log($"Changing the Ownership of the object: {ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playerData.playableState).name} to ClientId: {clientId}");
            } else
            {
                //Client is not registered, but already spawned
                Debug.LogError("CheckReconnect, Client is not registered, but already spawned, this is a error!");
            }

        } else if (ServerAuthenticationService.AuthIdToPlayerData.TryGetValue(userData.userAuthId, out PlayerData playerData))
        {
            //Already registered but not spawned
            Debug.Log("CheckReconnect, Already registered but not spawned");
            //Update clientId
            playerData.clientId = clientId;


        } else
        {
            //New client
            Debug.Log("CheckReconnect, New client");

            PlayerData newPlayerData = new PlayerData()
            {
                userData = userData,
                clientId = clientId,
                playableState = PlayableState.None, //None for now
                calculatedPearls = new CalculatedPearls(),
                gameObject = null
            };

            serverAuthenticationService.RegisterClient(newPlayerData);

        }
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
