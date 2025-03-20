using System;
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

    private MultiplayAllocationService multiplayAllocationService;

    public NetworkServer NetworkServer => networkServer;

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.serverQPort = queryPort;

        networkServer = new NetworkServer(networkManager, playerPrefab);

        multiplayAllocationService = new MultiplayAllocationService();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck(); //health check

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if(matchmakerPayload != null)
            {
                networkServer.OnUserLeft += UserLeft;
                networkServer.OnUserJoined += UserJoined;
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

        if (!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("NetworkServer did not start as expected.");
            return;
        }

        Loader.LoadNetwork(Loader.Scene.GameNetCodeTest);
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask) //pass tasks, when any completes, do the code
        {
            //true if our task finishes before the delay
            return matchmakerPayloadTask.Result;
        }
        return null;
    }

    private void UserJoined(UserData user)
    {
        multiplayAllocationService.AddPlayer();
    }

    private void UserLeft(UserData user)
    {
        multiplayAllocationService.RemovePlayer();

        //ShutdownServer
    }


    /// <summary>
    /// Call this to close the server. Match ended or all players quit.
    /// </summary>
    private async void ShutdownServer()
    {
        Dispose();
        Application.Quit();
    }

    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }


}
