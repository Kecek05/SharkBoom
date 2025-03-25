using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{

    private static ServerSingleton instance;


    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ServerSingleton>();

            if (instance == null)
            {
                //Debug.LogError("No ServerSingleton found in the scene.");
                return null;
            }

            return instance;
        }
    }

    private ServerGameManager gameManager;
    public ServerGameManager GameManager => gameManager;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateServer(NetworkObject playerPrefab)
    {
        await UnityServices.InitializeAsync();

        gameManager = new ServerGameManager(ApplicationData.IP(), ApplicationData.Port(), ApplicationData.QPort(), NetworkManager.Singleton, playerPrefab);
    }

    private void OnDestroy()
    {
        gameManager?.Dispose();
    }

}
