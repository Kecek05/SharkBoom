using Sortify;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static event Action OnLoadMainMenu;
    public event Action OnMatchmakingSearchStarted;
    public event Action OnMatchmakingCancelled;

    [BetterHeader("References")]
    [SerializeField] private Button openVsFriendsPanelBtn;




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
        //searchMatchmakingBtn.onClick.AddListener(StartMatchmaking);

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

    private void HostGameManager_OnFailToStartHost()
    {
        //createGameBtn.interactable = true;
    }



    private void OnDestroy()
    {
        HostGameManager.OnFailToStartHost -= HostGameManager_OnFailToStartHost;
    }
}
