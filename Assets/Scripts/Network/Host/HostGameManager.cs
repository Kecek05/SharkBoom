using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class HostGameManager : IDisposable //Actual Logic to interact with UGS (Relay, Lobby, etc)
{

    private NetworkServer networkServer;
    public NetworkServer NetworkServer => networkServer;
    private NetworkObject playerPrefab;

    public HostGameManager(NetworkObject _playerPrefab)
    {
        playerPrefab = _playerPrefab;
    }


    public async Task StartHostAsync()
    {
        Debug.Log("Starting Host");

        networkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab);

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene("GameNetCodeTest", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public async void ShutdownAsync()
    {
        networkServer?.Dispose();
    }

    public void Dispose()
    {
        ShutdownAsync();
    }
}
