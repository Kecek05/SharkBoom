using UnityEngine;

public class ApplicationLifecycle : MonoBehaviour
{
    public static ApplicationLifecycle instance;

    public static ApplicationLifecycle Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ApplicationLifecycle>();

            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            TryCancelMatchmaking();
        }
    }

    private void OnApplicationQuit()
    {
        TryCancelMatchmaking();
    }

    private async void TryCancelMatchmaking()
    {
        ClientSingleton client = ClientSingleton.Instance;

        if (client != null && client.GameManager != null)
        {
            await client.GameManager.CancelMatchmakingAsync();
        }
        else
        {
            Debug.LogWarning("Client or Client game manager is null");
        }
    }

}
