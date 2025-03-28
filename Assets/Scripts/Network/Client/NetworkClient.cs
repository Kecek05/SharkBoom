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
            if(GameManager.Instance != null)
            {
                //if (GameManager.Instance.GameStateManager.CurrentGameState.Value != GameState.GameEnded)
                //{
                //    //Game not ended yet
                //    GameManager.Instance.GameStateManager.ConnectionLostHostAndClient();
                //}
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

    private void IDisconnectedFromDS()
    {
        Debug.Log("I Disconnected from DS");
        if(SceneManager.GetActiveScene().name == Loader.Scene.Loading.ToString())
        {
            Debug.LogWarning("Disconnected in LoadingScene, closing the game");
            Disconnect();
            Application.Quit();
            return;
        }

        //Disconnect();
        //Application.Quit();
        //If the client disconects from DS, make a way to be possible to reconnect.

        //if (GameFlowManager.Instance != null)
        //{
        //    if (GameFlowManager.Instance.GameStateManager == null)
        //    {
        //        Disconnect();
        //        return;
        //    }

        //    //In Game Scene

        //    if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.None || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.WaitingForPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.SpawningPlayers || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.CalculatingResults)
        //    {
        //        //Game not started yet, go to menu
        //        Disconnect();
        //    }
        //    else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.ShowingPlayersInfo || GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameStarted)
        //    {
        //        //Game Started
        //        GameFlowManager.Instance.GameStateManager.GameOverAsync();
        //    }
        //    else if (GameFlowManager.Instance.GameStateManager.CurrentGameState.Value == GameState.GameEnded)
        //    {
        //        //Game Ended
        //        //Trigger Change Pearls, guarantee the change on pearls
        //        //await CalculatePearlsManager.TriggerChangePearls();
        //    }
        //} else
        //{
        //    Disconnect();
        //}
    }

    public void Disconnect()
    {
        Debug.Log("Client Disconnect");
        //Check if is host first
        if (networkManager != null && HostSingleton.Instance != null && networkManager.IsHost) //Server cant click buttons
        {
            HostSingleton.Instance.GameManager.ShutdownAsync();
        }

        if(networkManager.IsConnectedClient)
            networkManager.Shutdown();

        if (SceneManager.GetActiveScene().name != Loader.Scene.MainMenu.ToString())
        {
            Loader.Load(Loader.Scene.MainMenu);
        }
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            Disconnect();

            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

            networkManager.OnClientStarted -= NetworkManager_OnClientStarted;

            if(networkManager.SceneManager != null)
                networkManager.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
        }
    }


}
