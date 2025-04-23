using Sortify;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMatchmaking : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button searchMatchmakingBtn;
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private TMP_Text matchmakingTime;
    [SerializeField] private TMP_Text matchmakingText;

    private WaitForSeconds waitToTurnOnCancel = new WaitForSeconds(2f);
    private WaitForSeconds waitToIncreaseMatchmakingTime = new WaitForSeconds(1f);

    private Coroutine cancelButtonCoroutine;
    private Coroutine matchmakingTimerCoroutine;

    private bool isMatchMaking = false;
    private bool isCanceling = false;

    private float timeInQueue;


    private void Awake()
    {
        Hide();

        cancelMatchmakingBtn.onClick.AddListener(async () =>
        {
            //Cancel Matchmaking
            if (isCanceling) return;

            isCanceling = true;
            matchmakingText.text = "Canceling...";
            await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
            isMatchMaking = false;
            isCanceling = false;

            StopMatchmakingTimer();
            Hide();
        });

        searchMatchmakingBtn.onClick.AddListener(() =>
        {
            if (isCanceling) return;

            if (isMatchMaking) return;

            isMatchMaking = true;
            timeInQueue = 0f; // zera o tempo
            matchmakingText.text = "Searching...";
            ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); //We will pass and event to be trigger when the result is ready.

            StartMatchmakingTimer();
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

    private void Hide()
    {
        cancelMatchmakingBtn.interactable = false;
        matchmakingPanel.SetActive(false);
        matchmakingTime.text = string.Empty;
        matchmakingText.text = string.Empty;

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
        if(this == null) return; // Check for cancel this function if the object is destroyed

        StopMatchmakingTimer();

        switch (result)
        {
            case MatchmakerPollingResult.Success:
                // Debug.Log("Match Found Success!");
                matchmakingText.text = "Match Found Success!";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                // Debug.Log("MatchAssignmentError Error!");
                matchmakingText.text = "MatchAssignmentError Error!";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                // Debug.Log("TicketCreationError Error");
                matchmakingText.text = "TicketCreationError Error";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                // Debug.Log("TicketRetrievalError Error!");
                matchmakingText.text = "TicketRetrievalError Error!";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                // Debug.Log("TicketCancellationError Error!");
                matchmakingText.text = "TicketCancellationError Error!";
                break;
        }
    }

    private void OnDestroy()
    {
        MatchplayMatchmaker.OnTicketCreated -= MatchplayMatchmaker_OnTicketCreated;
    }
}
