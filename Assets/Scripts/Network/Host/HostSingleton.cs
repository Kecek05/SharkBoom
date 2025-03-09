using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("No HostSingleton found in the scene.");
                return null;
            }

            return instance;
        }
    }

    private HostGameManager gameManager;
    public HostGameManager GameManager => gameManager;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost(NetworkObject playerPrefab)
    {
        gameManager = new HostGameManager(playerPrefab);

    }

    private void OnDestroy()
    {
        gameManager?.Dispose();
    }
}
