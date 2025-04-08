using Sortify;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingPanelUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeReference] private MainMenuController mainMenuController;
    private WaitForSeconds waitToTurnOnCancel = new WaitForSeconds(2f);
    private Coroutine cancelButtonCoroutine;

    private void Awake()
    {
        Hide();

        cancelMatchmakingBtn.onClick.AddListener(() =>
        {
            //Cancel Matchmaking
            mainMenuController.CancelMatchmaking();
        });

        MatchplayMatchmaker.OnTicketCreated += MatchplayMatchmaker_OnTicketCreated;
    }

    private void MatchplayMatchmaker_OnTicketCreated()
    {
        if(cancelButtonCoroutine != null)
        {
            StopCoroutine(cancelButtonCoroutine);
            cancelButtonCoroutine = null;
        }

        cancelButtonCoroutine = StartCoroutine(CancelButtonDelay());
    }

    private IEnumerator CancelButtonDelay()
    {
        yield return waitToTurnOnCancel;
        cancelMatchmakingBtn.interactable = true;

        cancelButtonCoroutine = null;
    }

    private void Start()
    {
        mainMenuController.OnMatchmakingCancelled += Hide;
        mainMenuController.OnMatchmakingSearchStarted += Show;
    }


    private void Hide()
    {
        cancelMatchmakingBtn.interactable = false;
        matchmakingPanel.SetActive(false);

        if (cancelButtonCoroutine != null)
        {
            StopCoroutine(cancelButtonCoroutine);
            cancelButtonCoroutine = null;
        }
    }

    private void Show()
    {
        matchmakingPanel.SetActive(true);
    }
}
