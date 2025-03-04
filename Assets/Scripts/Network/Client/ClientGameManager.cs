using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ClientGameManager : IDisposable //Actual Logic to interact with UGS (Relay, Lobby, etc)
{
    private NetworkClient networkClient;

    public async Task InitAsync()
    {
        //Authenticate Player
    }

    public async Task StartClientAsync(string joinCode)
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Starting Client");
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
