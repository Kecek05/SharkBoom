using QFSW.QC;
using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject gameOverBackground;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI pearlsResultText;
    [SerializeField] private Image pearlsBackground;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Image returnBtnImage;
    [SerializeField] private Button returnBtn;
    [Space(5)]
    [BetterHeader("Win")]
    [SerializeField] private Sprite winBackground;
    [SerializeField] private Sprite winPearlsBackground;
    [SerializeField] private Sprite winReturnButton;
    [BetterHeader("Lose")]
    [SerializeField] private Sprite loseBackground;
    [SerializeField] private Sprite losePearlsBackground;
    [SerializeField] private Sprite loseReturnButton;
    [BetterHeader("Tie")]
    [SerializeField] private Sprite tieBackground;
    [SerializeField] private Sprite tiePearlsBackground;
    [SerializeField] private Sprite tieReturnButton;

    private bool alreadyChanged = false; //Prevent double change when losting connection

    private BaseGameOverManager gameOverManager;
    private BasePearlsManager pearlsManager;

    private void Awake()
    {
        Hide();

        alreadyChanged = false;

        returnBtn.onClick.AddListener(() =>
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
        Win();

        Debug.Log("Change GameOverUI to WIN");
    }

    private void GameStateManager_OnLose(bool isDraw)
    {
        if (alreadyChanged) return;

        alreadyChanged = true;

        //Lose UI Code
        if (isDraw)
        {
            Tie();
        }
        else
        {
            Lose();
        }

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

    private void Win()
    {
        ChangeUI("You Win!", "VICTORY!", winBackground, winPearlsBackground, winReturnButton);
    }

    private void Lose()
    {
        ChangeUI("You Lose!", "DEFEAT!", loseBackground, losePearlsBackground, loseReturnButton);
    }

    private void Tie()
    {
        ChangeUI("Time's Up!", "TIE!", tieBackground, tiePearlsBackground, tieReturnButton);
    }

    private void ChangeUI(string resultTxt, string resultTitleTxt, Sprite backgroundSprite, Sprite pearlsSprite, Sprite buttonSprite)
    {
        resultText.text = resultTxt;
        resultTitleText.text = resultTitleTxt;
        gameOverImage.sprite = backgroundSprite;
        pearlsBackground.sprite = pearlsSprite;
        returnBtnImage.sprite = buttonSprite;
    }

    private void OnDestroy()
    {
        gameOverManager.OnWin -= GameStateManager_OnWin;
        gameOverManager.OnLose -= GameStateManager_OnLose;
        pearlsManager.OnPearlsChanged -= PearlsManager_OnPearlsChanged;
    }
}
