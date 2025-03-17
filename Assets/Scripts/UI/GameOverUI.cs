using Sortify;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject gameOverBackground;
    [SerializeField] private TextMeshProUGUI playerResultText;
    [SerializeField] private TextMeshProUGUI pearlsResultText;
    [SerializeField] private Button returnButton;
    [SerializeField] private CalculatePearlsManager calculatePearlsManager;

    private void Awake()
    {
        returnButton.onClick.AddListener(() =>
        {
            //Return to main menu
            if(NetworkManager.Singleton.IsHost) //Server cant click buttons
            {
                HostSingleton.Instance.GameManager.ShutdownAsync();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        });
    }

    public override void OnNetworkSpawn()
    {
        Hide();

        if (IsClient)
        {
            GameFlowManager.Instance.LosePlayableState.OnValueChanged += LosePlayableStateLosePlay_OnValueChanged;
        }
    }

    private void LosePlayableStateLosePlay_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(newValue != PlayableState.None)
        {
            //Game Over
            SetupGameOver();
            Show();
        }
    }

    private void SetupGameOver()
    {
        if(GameFlowManager.Instance.LocalplayableState == GameFlowManager.Instance.LosePlayableState.Value)
        {
            playerResultText.text = "You Lose!";
            playerResultText.color = Color.red;

            pearlsResultText.text = calculatePearlsManager.GetPearls(false).ToString();
        } else
        {
            playerResultText.text = "You Win!";
            playerResultText.color = Color.green;

            pearlsResultText.text = calculatePearlsManager.GetPearls(true).ToString();
        }
    }

    private void Hide()
    {
        gameOverBackground.SetActive(false);
    }

    private void Show()
    {
        gameOverBackground.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        
    }
}
