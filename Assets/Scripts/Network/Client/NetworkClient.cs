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
            if(SceneManager.GetActiveScene().name == Loader.Scene.GameNetCodeTest.ToString())
            {
                //In Game Scene
                BaseGameStateManager baseGameStateManager = ServiceLocator.Get<BaseGameStateManager>();

                if (baseGameStateManager.CurrentGameState.Value != GameState.GameEnded)
                {
                    //Game not ended yet
                    baseGameStateManager.ConnectionLostHostAndClient();
                }
            } else
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

        if (SceneManager.GetActiveScene().name == Loader.Scene.GameNetCodeTest.ToString())
        {
            //In Game Scene

            if (ServiceLocator.Get<BaseGameStateManager>().CurrentGameState.Value != GameState.GameEnded)
            {
                //Game not ended yet
                Debug.LogWarning("Disconnected in Game DS, closing the game");
                Disconnect();
                Application.Quit();
            }
        }
        else
        {
            //Not in game scene
            Disconnect();
        }
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
            Debug.Log("NETCLIENT - Call shutdown on manager, but network client");
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
