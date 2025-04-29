using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable //Actual Logic to interact with UGS (Relay, Lobby, etc)
{
    public static event Action OnFailToStartHost;

    private const int MAX_CONNECTIONS = 2;

    private NetworkServer networkServer;
    private NetworkObject playerPrefab;

    private Allocation allocation;

    private string joinCode;
    public string JoinCode => joinCode;

    private string lobbyId;

    public HostGameManager(NetworkObject _playerPrefab)
    {
        playerPrefab = _playerPrefab;
    }

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        } catch (Exception e)
        {
            Debug.LogException(e);
            OnFailToStartHost?.Invoke();
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        } catch (Exception e)
        {
            Debug.LogException(e);
            OnFailToStartHost?.Invoke();
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new(allocation, "dtls");

        transport.SetRelayServerData(relayServerData);

        //Create the lobby, before .StartHost an after get joinCode

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value : joinCode)
                }
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"Player's Lobby", MAX_CONNECTIONS, lobbyOptions);

            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15f));

        } catch (LobbyServiceException lobbyEx)
        {
            Debug.LogException(lobbyEx);
            OnFailToStartHost?.Invoke();
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab);


        string payload = JsonUtility.ToJson(ClientSingleton.Instance.GameManager.UserData); //serialize the payload to json
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload); // serialize the payload to bytes

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        if(ClientSingleton.Instance != null)
        {
            ClientSingleton.Instance.GameManager.SetIsDedicatedServerGame(false);
        } else
        {
            Debug.LogError("ClientSingleton is null, couldn't set IsDedicatedServerGame to false");
        }

        NetworkManager.Singleton.StartHost();

        PearlsManager.OnFinishedCalculationsOnServer += PearlsManager_OnFinishedCalculationsOnServer;

        networkServer.OnClientLeft += HandleClientLeft;

        Loader.LoadHostNetwork(Loader.Scene.GameNetCodeTest);

        while(SceneManager.GetActiveScene().name != Loader.Scene.GameNetCodeTest.ToString())
        {
            //Not in game
            Debug.Log("Not in game scene");
            await Task.Delay(100);
        }

        Debug.Log("Loaded game scene");
        Debug.Log($"Loaded game scene, the game state is: {ServiceLocator.Get<BaseGameStateManager>().CurrentGameState.Value}");

        await Task.Delay(2000);
        Debug.Log("Waited to spawn players in host");

        networkServer.PlayerSpawner.SpawnPlayer();

        networkServer.PlayerSpawner.SpawnPlayer();

        await Task.Delay(1000); //Wait for a bit until can change ownership to prevent some bugs | IDK if is needed

        networkServer.SetCanChangeOwnership(true);

    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            if(authId != null || authId != string.Empty)
            {
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId); //Owner of the lobby is allowed to kick players
            }
        }
        catch (LobbyServiceException lobbyEx)
        {
            Debug.LogException(lobbyEx);
        }

        ServiceLocator.Get<BaseGameStateManager>().ConnectionLostHostAndClient();
        ShutdownAsync();
    }

    private IEnumerator HeartbeatLobby(float delayHeartbeatSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(delayHeartbeatSeconds); //optimization

        while(true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);

            yield return delay;
        }
    }

    /// <summary>
    /// Call this to shutdown the host. Doesn't go to Main Menu
    /// </summary>
    public async void ShutdownAsync()
    {
        if (string.IsNullOrEmpty(lobbyId)) return;


        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException lobbyEx)
        {
            Debug.LogException(lobbyEx);
        }
        lobbyId = string.Empty;

        networkServer.OnClientLeft -= HandleClientLeft;

        PearlsManager.OnFinishedCalculationsOnServer -= PearlsManager_OnFinishedCalculationsOnServer;

        networkServer?.Dispose();
        Debug.Log("NETMANAGER - Call network dispose on Host game manager");
    }

    private void PearlsManager_OnFinishedCalculationsOnServer()
    {
        Debug.Log("OnCanCloseServer on Host");
        ShutdownAsync();
    }
    public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }

    public void Dispose()
    {
        ShutdownAsync();
    }
}
