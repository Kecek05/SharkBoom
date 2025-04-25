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
    [SerializeField] private GameObject lobbyCodeErrorPanel;
    [SerializeField] private Button closeLobyCodeErrorPanelBtn;


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
            if (isBusy) return;

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
            if (isBusy) return;

            isBusy = true;
            lobbyCodeInputField.interactable = false;
            bool joinedSuccessfully = await ClientSingleton.Instance.GameManager.StartRelayClientAsync(lobbyCodeInputField.text);

            if (!joinedSuccessfully)
            {
                lobbyCodeErrorPanel.SetActive(true);
                isBusy = false;
                lobbyCodeInputField.text = "";
                lobbyCodeInputField.interactable = true;
            }
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

        closeLobyCodeErrorPanelBtn.onClick.AddListener(() =>
        {
            lobbyCodeErrorPanel.SetActive(false);
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
