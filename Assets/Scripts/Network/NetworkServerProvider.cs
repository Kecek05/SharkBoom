using UnityEngine;

public class NetworkServerProvider : MonoBehaviour
{
    private static NetworkServerProvider instance;

    public static NetworkServerProvider Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<NetworkServerProvider>();

            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    private NetworkServer currentNetworkServer;

    public NetworkServer CurrentNetworkServer
    {
        get
        {
            if(currentNetworkServer != null) return currentNetworkServer;

            currentNetworkServer = FindCurrentNetworkServer();

            if (currentNetworkServer == null)
            {
                Debug.LogError("No NetworkServer found in the scene.");
                return null;
            }
            Debug.Log($"Defined Current Network Server: {currentNetworkServer}");
            return currentNetworkServer;
        }
    }

    private NetworkServer FindCurrentNetworkServer()
    {
        if (HostSingleton.Instance != null)
        {
            return HostSingleton.Instance.GameManager.GetNetworkServer();
        }
        else if (ServerSingleton.Instance != null)
        {
            return ServerSingleton.Instance.GameManager.GetNetworkServer();
        }
        return null;
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyCurrentServer()
    {
        currentNetworkServer = null;
    }
}
