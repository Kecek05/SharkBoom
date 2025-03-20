using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour //Responsable for the client logic
{
    private static ClientSingleton instance;

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ClientSingleton>();

            if (instance == null)
            {
                //Debug.LogError("No ClientSingleton found in the scene.");
                return null;
            }

            return instance;
        }
    }



    private ClientGameManager gameManager;
    public ClientGameManager GameManager => gameManager;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateClient()
    {
        gameManager = new ClientGameManager();
    }

    public async Task<bool> AuthClientUnity()
    {
        return await GameManager.InitAsync(AuthTypes.Unity);
    }

    public async Task<bool> AuthAndroid()
    {
        return await GameManager.InitAsync(AuthTypes.Android);
    }

    public async Task<bool> AuthClientAnonymously()
    {
        return await GameManager.InitAsync(AuthTypes.Anonymous);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

}
