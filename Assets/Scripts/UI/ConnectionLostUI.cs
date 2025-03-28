using UnityEngine;
using UnityEngine.UI;
using Sortify;
using Unity.Netcode;

public class ConnectionLostUI : MonoBehaviour
{
    //ONLY FOR HOST AND CLIENT
    [BetterHeader("References")]
    [SerializeField] private GameObject connectionLostBackground;
    [SerializeField] private Button returnButton;

    private BaseGameStateManager gameStateManager;

    private void Awake()
    {
        Hide();

        returnButton.onClick.AddListener(() =>
        {
            //Return to main menu

            if (ClientSingleton.Instance != null)
                ClientSingleton.Instance.GameManager.Disconnect();
        });
    }

    private void Start()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();

        gameStateManager.OnLostConnectionInHost += GameStateManager_OnLostConnectionInHost;
    }

    private void GameStateManager_OnLostConnectionInHost()
    {
        Debug.Log("OnLostConnectionInHost in ConnectionLostUI");
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
