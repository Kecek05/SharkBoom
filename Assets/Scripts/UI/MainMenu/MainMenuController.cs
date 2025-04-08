using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static event Action OnLoadMainMenu;
    public event Action OnMatchmakingSearchStarted;

    [BetterHeader("References")]
    [SerializeField] private Button openVsFriendsPanelBtn;
    [SerializeField] private Button searchMatchmakingBtn;



    private bool isMatchMaking = false;
    private bool isCanceling = false;
    private bool isBusy = false;

    public bool IsMatchMaking => isMatchMaking;
    public bool IsCanceling => isCanceling;

    private async void Awake()
    {
        //openVsFriendsPanelBtn.onClick.AddListener(() =>
        //{
        //    vsFriendsPanel.SetActive(true);
        //});

        //closeVsFriendsPanelBtn.onClick.AddListener(() =>
        //{
        //    vsFriendsPanel.SetActive(false);
        //});

        //createGameBtn.onClick.AddListener(async () =>
        //{
        //    //createGameBtn.interactable = false;
        //    await HostSingleton.Instance.GameManager.StartHostAsync();
        //});


        //joinGameBtn.onClick.AddListener(async () =>
        //{
        //    //joinGameBtn.interactable = false;
        //    await ClientSingleton.Instance.GameManager.StartRelayClientAsync(lobbyCodeInputField.text);
        //});

        //quickJoinBtn.onClick.AddListener(async () =>
        //{
        //    //quickJoinBtn.interactable = false;
        //    await ClientSingleton.Instance.GameManager.QuickJoinLobbyAsync();
        //});

        ////CHANGE TO MACHMAKE LATER
        searchMatchmakingBtn.onClick.AddListener(StartMatchmaking);

        //cancelMatchmakingBtn.onClick.AddListener(async () =>
        //{
        //    if (isCanceling) return;
        //    isCanceling = true;
        //    Debug.Log("Canceling...");
        //    await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
        //    isMatchMaking = false;
        //    isCanceling = false;

        //    matchmakingPanel.SetActive(false);
        //});


    }

    private void Start()
    {
        OnLoadMainMenu?.Invoke();

        HostGameManager.OnFailToStartHost += HostGameManager_OnFailToStartHost;

    }

    private void StartMatchmaking()
    {
        if (isCanceling) return;

        if (isMatchMaking) return;

        //if(isMatchMaking)
        //{
        //    isCanceling = true;
        //    Debug.Log("Canceling...");
        //    await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
        //    isMatchMaking = false;
        //    isCanceling = false;
        //    return;
        //}

        Debug.Log("Searching...");
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); //We will pass and event to be trigger when the result is ready.
        isMatchMaking = true;

        OnMatchmakingSearchStarted?.Invoke();

        //matchmakingPanel.SetActive(true);
    }


    public async void CancelMatchmaking()
    {
        if (isCanceling) return;

        isCanceling = true;
        Debug.Log("Canceling...");
        await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
        isMatchMaking = false;
        isCanceling = false;
    }

    private void HostGameManager_OnFailToStartHost()
    {
        //createGameBtn.interactable = true;
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        switch(result)
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
        HostGameManager.OnFailToStartHost -= HostGameManager_OnFailToStartHost;
    }
}
