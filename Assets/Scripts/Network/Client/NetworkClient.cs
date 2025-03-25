using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable //Actual Client Game Logic
{

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager) // our constructor
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        networkManager.OnClientStarted += NetworkManager_OnClientStarted;

    }

    private void NetworkManager_OnClientStarted()
    {
        networkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        Debug.Log($"Scene Event: {sceneEvent.SceneEventType}");
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(!ClientSingleton.Instance.GameManager.IsDedicatedServerGame)
        {
            //Host
            if(GameFlowManager.Instance != null)
            {
                if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value != GameState.GameEnded)
                {
                    //Game not ended yet
                    GameFlowManager.Instance.GameStateManager.ConnectionLostHostAndClient();
                }
            } 
            else
            {
                //Not in game scene
                Disconnect();
            }
        }  else
        {
            //Is Dedicated Server
            if (clientId == networkManager.LocalClientId)
            {
                IDisconnectedFromDS();
            }
        }
    }

    private async void IDisconnectedFromDS()
    {
        Debug.Log("I Disconnected from DS");

        if (GameFlowManager.Instance != null)
        {
            if (GameFlowManager.Instance.GameStateManager == null)
            {
                Disconnect();
                return;
            }

            //In Game Scene

            if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.None || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.WaitingForPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.SpawningPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.CalculatingResults)
            {
                //Game not started yet, go to menu
                Disconnect();
            }
            else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.ShowingPlayersInfo || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameStarted)
            {
                //Game Started
                GameFlowManager.Instance.GameStateManager.GameOverAsync();
            }
            else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameEnded)
            {
                //Game Ended
                //Trigger Change Pearls, guarantee the change on pearls
                await CalculatePearlsManager.TriggerChangePearls();
            }
        } else
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        //Check if is host first
        if (networkManager != null && HostSingleton.Instance != null && networkManager.IsHost) //Server cant click buttons
        {
            HostSingleton.Instance.GameManager.ShutdownAsync();
        }

        if (SceneManager.GetActiveScene().name != Loader.Scene.MainMenu.ToString())
        {
            Loader.Load(Loader.Scene.MainMenu);
        }

        if(networkManager.IsConnectedClient)
            networkManager.Shutdown();
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

            networkManager.OnClientStarted -= NetworkManager_OnClientStarted;

            if(networkManager.SceneManager != null)
                networkManager.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
        }
    }


}
