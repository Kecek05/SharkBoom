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

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask) //pass tasks, when any completes, do the code
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

        //ServerSingleton.Instance.GameManager.ShutdownServerDelayed();


        if (GameFlowManager.Instance != null)
        {
            if (GameFlowManager.Instance.GameStateManager == null)
            {
                ServerSingleton.Instance.GameManager.ShutdownServerDelayed();
                return;
            }

            //In Game Scene

            if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.None || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.WaitingForPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.SpawningPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.CalculatingResults)
            {
                //Game not started yet, Shutdown Server
                ServerSingleton.Instance.GameManager.ShutdownServerDelayed();
            }
            else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.ShowingPlayersInfo || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameStarted)
            {
                //Game Started
                GameFlowManager.Instance.GameStateManager.LoseGame(PlayableState.PlayerQuited);

                ServerSingleton.Instance.GameManager.ShutdownServerDelayed();
            }
            else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameEnded)
            {
                //Game Ended

                if (ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
                {
                    //Dedicated, Trigger Change Pearls, guarantee the change on pearls
                    //await CalculatePearlsManager.TriggerChangePearls();
                }
                else
                {
                    //Host. Do nothing
                }
            }
        }
    }
 #endif

    /// <summary>
    /// Call this to close the server. Match ended or all players quit.
    /// </summary>
    private async void ShutdownServerDelayed(int delay = 3000) //default 3 seconds
    {
        await Task.Delay(delay);
    
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
        //multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }


}
