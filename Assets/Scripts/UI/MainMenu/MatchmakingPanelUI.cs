using Sortify;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingPanelUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeReference] private MainMenuController mainMenuController;

    private void Awake()
    {
        Hide();

        cancelMatchmakingBtn.onClick.AddListener(() =>
        {
            //Cancel Matchmaking
            mainMenuController.CancelMatchmaking();
        });
    }

    private void Start()
    {
        mainMenuController.OnMatchmakingCancelled += Hide;
        mainMenuController.OnMatchmakingSearchStarted += Show;
    }


    private void Hide()
    {
        matchmakingPanel.SetActive(false);
    }

    private void Show()
    {
        matchmakingPanel.SetActive(true);
    }
}
