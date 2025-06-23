using UnityEngine;
using UnityEngine.UI;
using Sortify;
using Unity.Netcode;

public class ConnectionLostUI : MonoBehaviour
{
    //ONLY FOR HOST AND CLIENT
    [BetterHeader("References")]
    [SerializeField] private GameObject connectionLostBackground;

    private BaseGameStateManager gameStateManager;
    
    private void Awake()
    {
        Hide();
    }

    private void Start()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        gameStateManager.OnLostConnectionInHost += GameStateManager_OnLostConnectionInHost;
    }

    public void ReturnGame()
    {
        if (ClientSingleton.Instance != null)
            ClientSingleton.Instance.GameManager.Disconnect();
    }

    public void HostReturnMenu()
    {
        if(HostSingleton.Instance != null)
            HostSingleton.Instance.GameManager.ShutdownAsync();
    }

    private void GameStateManager_OnLostConnectionInHost()
    {
        Show();
    }

    private void Hide()
    {
        connectionLostBackground.SetActive(false);
    }

    private void Show()
    {
        connectionLostBackground.SetActive(true);
    }

    private void OnDestroy()
    {
        gameStateManager.OnLostConnectionInHost -= GameStateManager_OnLostConnectionInHost;
    }
}
