using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkClient : IDisposable //Actual Client Game Logic
{

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager) // our constructor
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("Disconnected");
    }

    public void Dispose()
    {
        if (networkManager != null)
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
