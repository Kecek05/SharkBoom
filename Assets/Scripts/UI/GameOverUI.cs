using QFSW.QC;
using Sortify;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject gameOverBackground;
    [SerializeField] private TextMeshProUGUI playerResultText;
    [SerializeField] private TextMeshProUGUI pearlsResultText;
    [SerializeField] private Button returnButton;

    private bool alreadyChanged = false; //Prevent double change when losting connection

    private BaseGameOverManager gameOverManager;
    private void Awake()
    {
        Hide();

        alreadyChanged = false;

        returnButton.onClick.AddListener(() =>
        {
            //Return to main menu

            if(ClientSingleton.Instance != null)
                ClientSingleton.Instance.GameManager.Disconnect();
        });

    }

    private void Start()
    {
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();

        gameOverManager.OnWin += GameStateManager_OnWin;
        gameOverManager.OnLose += GameStateManager_OnLose;

        ////CalculatePearlsManager.OnPearlsDeltaChanged += CalculatePearlsManager_OnPearlsDeltaChanged;
        //gameOverManager.OnCanShowPearls += GameStateManager_OnCanShowPearls;
        //GameFlowManager.Instance.GameStateManager.OnGameOver += GameStateManager_OnGameOver;
    }

    private void GameStateManager_OnCanShowPearls(int pearlsToShow)
    {
        SetupPearlsResult(pearlsToShow);
        Show();
    }

    //private void GameStateManager_OnGameOver()
    //{
    //    Debug.Log("GameOverUI: GameStateManager_OnGameOver");
    //    Show();
    //}

    //private void CalculatePearlsManager_OnPearlsDeltaChanged(int pearlsDelta)
    //{
    //    //Pearls value to show changed, show UI.
    //    SetupPearlsResult(pearlsDelta);
    //}

    private void SetupPearlsResult(int pearlsDelta)
    {
        if (pearlsDelta == 0)
        {
            //Relay game, no pearls to show
            pearlsResultText.gameObject.SetActive(false);
        }
        else if (pearlsDelta > 0)
        {
            //Win
            pearlsResultText.text = "+" + pearlsDelta.ToString();
        }
        else
        {
            //Lose
            pearlsResultText.text = pearlsDelta.ToString();
        }
    }

    private void GameStateManager_OnWin()
    {
        if(alreadyChanged) return;

        alreadyChanged = true;

        //Win UI Code

        playerResultText.text = "You Win!";
        playerResultText.color = Color.green;

        Debug.Log("Change GameOverUI to WIN");
    }

    private void GameStateManager_OnLose()
    {
        if (alreadyChanged) return;

        alreadyChanged = true;

        //Lose UI Code

        playerResultText.text = "You Lose!";
        playerResultText.color = Color.red;

        Debug.Log("Change GameOverUI to Lose");
    }

    private void Hide()
    {
        gameOverBackground.SetActive(false);
    }

    [Command("gameOverUI-show")]
    private void Show()
    {
        gameOverBackground.SetActive(true);
    }

    private void OnDestroy()
    {
        gameOverManager.OnWin -= GameStateManager_OnWin;
        gameOverManager.OnLose -= GameStateManager_OnLose;

        //GameManager.Instance.GameStateManager.OnCanShowPearls -= GameStateManager_OnCanShowPearls;
        ////CalculatePearlsManager.OnPearlsDeltaChanged -= CalculatePearlsManager_OnPearlsDeltaChanged;
    }
}
