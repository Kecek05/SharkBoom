using UnityEngine;
using UnityEngine.UI;
using Sortify;
using Unity.Netcode;

public class ConnectionLostUI : NetworkBehaviour
{
    //ONLY FOR HOST AND CLIENT
    [BetterHeader("References")]
    [SerializeField] private GameObject connectionLostBackground;
    [SerializeField] private Button returnButton;

    private void Awake()
    {
        returnButton.onClick.AddListener(() =>
        {
            //Return to main menu

            if (ClientSingleton.Instance != null)
                ClientSingleton.Instance.GameManager.Disconnect();
        });
    }

    public override void OnNetworkSpawn()
    {
        Hide();

        if(IsClient)
        {
            GameFlowManager.Instance.GameStateManager.OnConnectionLost += GameStateManager_OnConnectionLost;
        }
    }

    private void GameStateManager_OnConnectionLost()
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

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            GameFlowManager.Instance.GameStateManager.OnConnectionLost -= GameStateManager_OnConnectionLost;
        }
    }
}
