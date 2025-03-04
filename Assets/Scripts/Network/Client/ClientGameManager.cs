using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable //Actual Logic to interact with UGS (Relay, Lobby, etc)
{
    private NetworkClient networkClient;

    private const string MENU_SCENE = "MainMenu";

    public async Task InitAsync()
    {
        //Authenticate Player
    }

    public async Task StartClientAsync(string joinCode)
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Starting Client");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
