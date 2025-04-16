using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuVsFriends : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button openVsFriendsPanelBtn;
    [SerializeField] private Button closeVsFriendsPanelBtn;
    [SerializeField] private Button joinGameBtn;
    [SerializeField] private Button createGameBtn;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private GameObject vsFriendsPanel;

    private bool isBusy = false;

    private void Awake()
    {
        Hide();

        openVsFriendsPanelBtn.onClick.AddListener(() =>
        {
            Show();
        });

        closeVsFriendsPanelBtn.onClick.AddListener(() =>
        {
            if(isBusy) return;

            Hide();
        });

        createGameBtn.onClick.AddListener(async () =>
        {
            if (isBusy) return;

            isBusy = true;
            createGameBtn.interactable = false;
            await HostSingleton.Instance.GameManager.StartHostAsync();
            //createGameBtn.interactable = true;
            isBusy = false;
        });

        joinGameBtn.onClick.AddListener(async () =>
        {
            if(isBusy) return;

            isBusy = true;
            lobbyCodeInputField.interactable = false;
            await ClientSingleton.Instance.GameManager.StartRelayClientAsync(lobbyCodeInputField.text);
            //lobbyCodeInputField.interactable = true;
            isBusy = false;

        });

        quickJoinBtn.onClick.AddListener(async () =>
        {
            if (isBusy) return;
            //quickJoinBtn.interactable = false;
            isBusy = true;
            await ClientSingleton.Instance.GameManager.QuickJoinLobbyAsync();
            isBusy = false;
        });

    }


    private void Hide()
    {
        vsFriendsPanel.SetActive(false);
    }

    public void Show()
    {
        vsFriendsPanel.SetActive(true);
    }
}
