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
            Loader.Load(Loader.Scene.MainMenu);
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

            pearlsResultText.text = calculatePearlsManager.GetCalculatePearls(false).ToString();
        } else
        {
            playerResultText.text = "You Win!";


            pearlsResultText.text = calculatePearlsManager.GetCalculatePearls(true).ToString();
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
