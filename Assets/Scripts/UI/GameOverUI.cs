using QFSW.QC;
using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject gameOverBackground;
    [SerializeField] private TextMeshProUGUI playerResultText;
    [SerializeField] private TextMeshProUGUI pearlsResultText;
    [SerializeField] private Button returnButton;
    [SerializeField] private Color drawColor;

    private bool alreadyChanged = false; //Prevent double change when losting connection

    private BaseGameOverManager gameOverManager;
    private BasePearlsManager pearlsManager;

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
        pearlsManager = ServiceLocator.Get<BasePearlsManager>();

        gameOverManager.OnWin += GameStateManager_OnWin;
        gameOverManager.OnLose += GameStateManager_OnLose;

        pearlsManager.OnPearlsChanged += PearlsManager_OnPearlsChanged;
    }

    private void PearlsManager_OnPearlsChanged(int pearlsToShow)
    {
        SetupPearlsResult(pearlsToShow);
        Show();
    }

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

    private void GameStateManager_OnLose(bool isDraw)
    {
        if (alreadyChanged) return;

        alreadyChanged = true;

        //Lose UI Code
        if(isDraw)
        {
            playerResultText.text = "Draw!";
            playerResultText.color = drawColor;
        }

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
        pearlsManager.OnPearlsChanged -= PearlsManager_OnPearlsChanged;
    }
}
