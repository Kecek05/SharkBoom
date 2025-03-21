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

    public NetworkServer CurrentNetworkServer => currentNetworkServer;

    private void FindCurrentNetworkServer()
    {
        if (HostSingleton.Instance != null)
        {
            currentNetworkServer =  HostSingleton.Instance.GameManager.GetNetworkServer();
        }
        else if (ServerSingleton.Instance != null)
        {
            currentNetworkServer =  ServerSingleton.Instance.GameManager.GetNetworkServer();
        }

        Debug.Log($"Current Network Server: {currentNetworkServer}");
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        FindCurrentNetworkServer();
    }

}
