using Sortify;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMatchmaking : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button searchMatchmakingBtn;
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;
    private WaitForSeconds waitToTurnOnCancel = new WaitForSeconds(2f);
    private Coroutine cancelButtonCoroutine;

    private bool isMatchMaking = false;
    private bool isCanceling = false;

    private void Awake()
    {
        Hide();

        cancelMatchmakingBtn.onClick.AddListener(async () =>
        {
            //Cancel Matchmaking
            if (isCanceling) return;

            isCanceling = true;
            Debug.Log("Canceling...");
            await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
            isMatchMaking = false;
            isCanceling = false;

            Hide();
        });

        searchMatchmakingBtn.onClick.AddListener(() =>
        {
            if (isCanceling) return;

            if (isMatchMaking) return;

            isMatchMaking = true;
            Debug.Log("Searching...");
            ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); //We will pass and event to be trigger when the result is ready.

            Show();

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

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                Debug.Log("Match Found Success!");
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                Debug.Log("MatchAssignmentError Error!");
                break;
            case MatchmakerPollingResult.TicketCreationError:
                Debug.Log("TicketCreationError Error");
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                Debug.Log("TicketRetrievalError Error!");
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                Debug.Log("TicketCancellationError Error!");
                break;
        }
    }

    private void OnDestroy()
    {
        MatchplayMatchmaker.OnTicketCreated -= MatchplayMatchmaker_OnTicketCreated;
    }
}
