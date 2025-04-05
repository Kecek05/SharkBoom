using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ServerGameManager : IDisposable
{

    private string serverIP;
    private int serverPort;
    private int serverQPort;

    private NetworkServer networkServer;

#if UNITY_SERVER
    private MultiplayAllocationService multiplayAllocationService;
#endif

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = queryPort;

        networkServer = new NetworkServer(networkManager, playerPrefab);
#if UNITY_SERVER
        multiplayAllocationService = new MultiplayAllocationService();
#endif

        PearlsManager.OnFinishedCalculationsOnServer += Pearls_OnFinishedCalculationsOnServer;

        ServiceLocatorBootstrap.OnServiceLocatorInitialized += ServiceLocatorBootstrap_OnServiceLocatorInitialized;
    }



    public async Task StartGameServerAsync()
    {
#if UNITY_SERVER
        await multiplayAllocationService.BeginServerCheck(); //health check


        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if(matchmakerPayload != null)
            {
                networkServer.OnUserLeft += UserLeft;
                networkServer.OnUserJoined += UserJoined;
                HandleMatchmakerPayload(matchmakerPayload);

            } else
            {
                Debug.LogError("Failed to get matchmaker payload. Timed out");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }
#endif
        if (!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("NetworkServer did not start as expected.");
            return;
        }
    }

 #if UNITY_SERVER

    private async void HandleMatchmakerPayload(MatchmakingResults matchmakerPayload)
    {
        Dictionary<string, int> authIdToPearlsPayload = new Dictionary<string, int>();

        foreach (Player player in matchmakerPayload.MatchProperties.Players)
        {
            Dictionary<string, int> customDataDictionary = player.CustomData.GetAs<Dictionary<string, int>>();

            customDataDictionary.TryGetValue("pearls", out int pearls);

            authIdToPearlsPayload[player.Id] = pearls;

            await Reconnect.SetIsInMatch(player.Id, true);

            await Reconnect.SetPlayerMatchConnection(player.Id, multiplayAllocationService.GetMultiplayService.ServerConfig.IpAddress, multiplayAllocationService.GetMultiplayService.ServerConfig.Port);

            Debug.Log($"PlayerId: {player.Id} - Pearls: {pearls}");

        }

        string player1AuthId = matchmakerPayload.MatchProperties.Players[0].Id;
        string player2AuthId = matchmakerPayload.MatchProperties.Players[1].Id;

        int player1Pearls = authIdToPearlsPayload[player1AuthId];
        int player2Pearls = authIdToPearlsPayload[player2AuthId];

        CalculatePearls.CalculatePossibleResultsWihAllocation(player1AuthId, player2AuthId, player1Pearls, player2Pearls);


        await Task.Delay(2000); //change to wait for some callback, I think might be an OnServerStarted | need to wait the server loads de scene and the Game Manager Handle Events
        Debug.Log("Waited Delay to spawn players");

        networkServer.PlayerSpawner.SpawnPlayer();

        networkServer.PlayerSpawner.SpawnPlayer();

        networkServer.SetCanChangeOwnership(true);
    }

    private void ServiceLocatorBootstrap_OnServiceLocatorInitialized()
    {
        Debug.Log("ServiceLocatorBootstrap_OnServiceLocatorInitialized, Spawning Players by server");

        //networkServer.PlayerSpawner.SpawnPlayer();

        //networkServer.PlayerSpawner.SpawnPlayer();
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(600000)) == matchmakerPayloadTask) //pass tasks, when any completes, do the code
        {
            //true if our task finishes before the delay
            return matchmakerPayloadTask.Result;
        }
        return null;
    }


    private void UserJoined()
    {
        multiplayAllocationService.AddPlayer();
    }

    private void UserLeft()
    {
        multiplayAllocationService.RemovePlayer();

        Debug.Log($"User Left");

    }

#endif

    private async void Pearls_OnFinishedCalculationsOnServer()
    {
        foreach(PlayerData playerData in NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas)
        {
            await Reconnect.SetIsInMatch(playerData.userData.userAuthId, false);
        }

        ShutdownServer();
    }

    /// <summary>
    /// Call this to close the server. Match ended or all players quit.
    /// </summary>
    public void ShutdownServer()
    {
        Debug.Log("SHUTING DOWN SERVER");
        Dispose();
        Application.Quit();
    }

    public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }

    public void Dispose()
    {
#if UNITY_SERVER
        multiplayAllocationService?.Dispose();
#endif
        PearlsManager.OnFinishedCalculationsOnServer -= Pearls_OnFinishedCalculationsOnServer;
        ServiceLocatorBootstrap.OnServiceLocatorInitialized -= ServiceLocatorBootstrap_OnServiceLocatorInitialized;
        networkServer?.Dispose();
    }


}
