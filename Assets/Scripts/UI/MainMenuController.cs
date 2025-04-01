using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static event Action OnLoadMainMenu;

    [BetterHeader("References")]
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button lobbiesButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button quickJoinButton;

    private bool isMatchMaking = false;
    private bool isCanceling = false;

    private async void Awake()
    {
        hostButton.onClick.AddListener(async () =>
        {
            hostButton.interactable = false;
            await HostSingleton.Instance.GameManager.StartHostAsync();
            hostButton.interactable = true;
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        //clientButton.onClick.AddListener(async() =>
        //{
        //    clientButton.interactable = false;
        //    await ClientSingleton.Instance.GameManager.StartRelayClientAsync(lobbyCodeInputField.text);
        //    clientButton.interactable = true;
        //});

        quickJoinButton.onClick.AddListener(async () =>
        {
            quickJoinButton.interactable = false;
            await ClientSingleton.Instance.GameManager.QuickJoinLobbyAsync();
            quickJoinButton.interactable = true;
        });

        //CHANGE TO MACHMAKE LATER
        clientButton.onClick.AddListener(async () =>
        {
            if (isCanceling) return;

            if(isMatchMaking)
            {
                isCanceling = true;
                Debug.Log("Canceling...");
                await ClientSingleton.Instance.GameManager.CancelMatchmakingAsync(); //wait to cancel the matchmake
                isMatchMaking = false;
                isCanceling = false;
                return;
            }

            Debug.Log("Searching...");
            ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade); //We will pass and event to be trigger when the result is ready.
            isMatchMaking = true;

        });
    }

    private void Start()
    {
        OnLoadMainMenu?.Invoke();
    }

    private void Update()
    {
        //DEBUG
        if(ClientSingleton.Instance != null)
        {
            lobbyCodeInputField.text = ClientSingleton.Instance.GameManager.UserData.userPearls.ToString();
        }
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
}
