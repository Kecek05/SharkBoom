using QFSW.QC;
using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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
    [SerializeField] private Image animateBackground;
    [SerializeField] private VideoPlayer gameOverVideoPlayer;
    [BetterHeader("Renders")]
    [SerializeField] private VideoClip orcaWin;
    [SerializeField] private VideoClip orcaLose;
    [SerializeField] private VideoClip sharkWin;
    [SerializeField] private VideoClip sharkLose;
    [Space(5)]
    [BetterHeader("Win")]
    [SerializeField] private Material winBackgroundMaterial;
    [SerializeField] private Sprite winBackground;
    [SerializeField] private Sprite winPearlsBackground;
    [SerializeField] private Sprite winReturnButton;
    [BetterHeader("Lose")]
    [SerializeField] private Material loseBackgroundMaterial;
    [SerializeField] private Sprite loseBackground;
    [SerializeField] private Sprite losePearlsBackground;
    [SerializeField] private Sprite loseReturnButton;
    [BetterHeader("Tie")]
    [SerializeField] private Material tieBackgroundMaterial;
    [SerializeField] private Sprite tieBackground;
    [SerializeField] private Sprite tiePearlsBackground;
    [SerializeField] private Sprite tieReturnButton;

    

    private bool alreadyChanged = false; //Prevent double change when losting connection

    private BaseGameOverManager gameOverManager;
    private BasePearlsManager pearlsManager;
    private BasePlayersPublicInfoManager playersPublicInfoManager;
    private BaseTurnManager turnManager;

    private const string TEXTANIMATOR_WIN = "<win>";
    private const string TEXTANIMATOR_LOSE = "<lose>";

    private void Awake()
    {
        Hide();
        alreadyChanged = false;
    }

    private void Start()
    {
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();
        pearlsManager = ServiceLocator.Get<BasePearlsManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        gameOverManager.OnWin += GameStateManager_OnWin;
        gameOverManager.OnLose += GameStateManager_OnLose;

        pearlsManager.OnPearlsChanged += PearlsManager_OnPearlsChanged;
    }

    public void ReturnToMenu()
    {
        if (ClientSingleton.Instance != null)
            gameOverVideoPlayer.Stop();
            ClientSingleton.Instance.GameManager.Disconnect();
    }
    private void PearlsManager_OnPearlsChanged(int pearlsToShow)
    {
        SetupPearlsResult(pearlsToShow);
        Show();

        playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
    }

    private void SetupPearlsResult(int pearlsDelta)
    {
        if (pearlsDelta == 0)
        {
            //Relay game, no pearls to show
            pearlsResultText.gameObject.SetActive(false);
            pearlsBackground.gameObject.SetActive(false);
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
        gameOverVideoPlayer.clip = GetGameOverRender(playersPublicInfoManager.GetPlayerVisualTypes()[turnManager.LocalPlayableState], GameResult.Win);
        gameOverVideoPlayer.Play();

        ChangeUI($"{TEXTANIMATOR_WIN}You Win!{TEXTANIMATOR_WIN}", "VICTORY!", winBackground, winPearlsBackground, winReturnButton, winBackgroundMaterial);
    }

    private void Lose()
    {
        gameOverVideoPlayer.clip = GetGameOverRender(playersPublicInfoManager.GetPlayerVisualTypes()[turnManager.LocalPlayableState], GameResult.Lose);
        gameOverVideoPlayer.Play();

        ChangeUI($"{TEXTANIMATOR_LOSE}You Lose!{TEXTANIMATOR_LOSE}", "DEFEAT!", loseBackground, losePearlsBackground, loseReturnButton, loseBackgroundMaterial);
    }

    private void Tie()
    {
        gameOverVideoPlayer.clip = GetGameOverRender(playersPublicInfoManager.GetPlayerVisualTypes()[turnManager.LocalPlayableState], GameResult.Tie);
        gameOverVideoPlayer.Play();

        ChangeUI("Time's Up!", "TIE!", tieBackground, tiePearlsBackground, tieReturnButton, tieBackgroundMaterial);
    }

    private void ChangeUI(string resultTxt, string resultTitleTxt, Sprite backgroundSprite, Sprite pearlsSprite, Sprite buttonSprite, Material animateMaterial)
    {
        resultText.text = resultTxt;
        resultTitleText.text = resultTitleTxt;
        gameOverImage.sprite = backgroundSprite;
        pearlsBackground.sprite = pearlsSprite;
        returnBtnImage.sprite = buttonSprite;
        animateBackground.material = animateMaterial;
    }

    private VideoClip GetGameOverRender(PlayerVisualType visualType, GameResult result)
    {
        switch (result)
        {
            case GameResult.Win:
                switch(visualType)
                {
                    case PlayerVisualType.Shark:
                        return sharkWin;
                    case PlayerVisualType.Orca:
                        return orcaWin;
                }
                break;
            case GameResult.Lose:
            case GameResult.Tie:
                switch(visualType)
                {
                    case PlayerVisualType.Shark:
                        return sharkLose;
                    case PlayerVisualType.Orca:
                        return orcaLose;
                }
                break;
        }

        return null;
    }

    private void OnDestroy()
    {
        gameOverManager.OnWin -= GameStateManager_OnWin;
        gameOverManager.OnLose -= GameStateManager_OnLose;
        pearlsManager.OnPearlsChanged -= PearlsManager_OnPearlsChanged;
    }
}

public enum GameResult
{
    Win,
    Lose,
    Tie
}
