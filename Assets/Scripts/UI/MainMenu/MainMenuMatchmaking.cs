using Sortify;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuMatchmaking : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button searchMatchmakingBtn;
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private TMP_Text matchmakingTime;
    [SerializeField] private TMP_Text matchmakingText;

    private WaitForSeconds waitToTurnOnSearch = new WaitForSeconds(2f);
    private WaitForSeconds waitToIncreaseMatchmakingTime = new WaitForSeconds(1f);

    private Coroutine searchButtonCoroutine;
    private Coroutine matchmakingTimerCoroutine;

    private bool isMatchMaking = false;
    private bool isCanceling = false;

    private float timeInQueue;
    private const string TEXTANIMATOR_SEARCHING = "<loading>";


    private void Awake()
    {
        Hide();
        MatchplayMatchmaker.OnTicketCreated += MatchplayMatchmaker_OnTicketCreated;
    }

    public async void CancelMatchmaking()
    {
        //Cancel Matchmaking
        if (isCanceling || !isMatchMaking) return;

        isCanceling = true;
        matchmakingText.text = $"{TEXTANIMATOR_SEARCHING}Canceling match...{TEXTANIMATOR_SEARCHING}";
        await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync();
        CanceledMatchmaking();
    }

    public void SearchMathmaking()
    {
        if (isCanceling || isMatchMaking) return;

        isMatchMaking = true;
        timeInQueue = 0f;
        matchmakingText.text = $"{TEXTANIMATOR_SEARCHING}Searching for players...{TEXTANIMATOR_SEARCHING}";
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); // We will pass and event to be trigger when the result is ready.
        StartMatchmakingTimer();
        Show();
    }

    private void MatchplayMatchmaker_OnTicketCreated()
    {
        cancelMatchmakingBtn.interactable = true;
    }

    private void StartMatchmakingTimer()
    {
        if (matchmakingTimerCoroutine != null)
        {
            StopCoroutine(matchmakingTimerCoroutine);
        }

        matchmakingTimerCoroutine = StartCoroutine(MatchmakingTimer());
    }

    private void StopMatchmakingTimer()
    {
        if (matchmakingTimerCoroutine != null)
        {
            StopCoroutine(matchmakingTimerCoroutine);
            matchmakingTimerCoroutine = null;
        }

        matchmakingTime.text = string.Empty;
    }

    private IEnumerator MatchmakingTimer()
    {
        while (isMatchMaking)
        {
            timeInQueue += 1f;
            TimeSpan ts = TimeSpan.FromSeconds(timeInQueue);
            matchmakingTime.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            yield return waitToIncreaseMatchmakingTime;
        }
    }

    private void CanceledMatchmaking()
    {
        isMatchMaking = false;
        isCanceling = false;
        StopMatchmakingTimer();
        Hide();
    }

    private void Hide()
    {
        cancelMatchmakingBtn.interactable = false;
        matchmakingPanel.SetActive(false);
        matchmakingTime.text = string.Empty;
        matchmakingText.text = string.Empty;
        searchMatchmakingBtn.interactable = false;

        if (searchButtonCoroutine != null)
        {
            StopCoroutine(searchButtonCoroutine);
            searchButtonCoroutine = null;
        }

        searchButtonCoroutine = StartCoroutine(SearchButtonDelay());
    }


    private IEnumerator SearchButtonDelay()
    {
        yield return waitToTurnOnSearch;
        searchMatchmakingBtn.interactable = true;
    }

    private void Show()
    {
        matchmakingPanel.SetActive(true);
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        if(this == null) return; // Check for cancel this function if the object is destroyed

        StopMatchmakingTimer();

        switch (result)
        {
            case MatchmakerPollingResult.Success:
                matchmakingText.text = "Match Found Success!";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                matchmakingText.text = "Failed to join the match!";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                matchmakingText.text = "Failed to start matchmaking!";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                matchmakingText.text = "Match Canceled!";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                matchmakingText.text = "Match can't be canceled!";
                break;
        }
    }

    private void OnDestroy()
    {
        MatchplayMatchmaker.OnTicketCreated -= MatchplayMatchmaker_OnTicketCreated;
    }

    
}
