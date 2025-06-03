using MoreMountains.Feedbacks;
using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuVsFriends : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button createGameBtn;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private GameObject vsFriendsPanel;
    [SerializeField] private GameObject lobbyCodeErrorPanel;


    private bool isBusy = false;

    private void Awake()
    {
        Hide();
    }
    
    public void OpenVsFriendsPanel()
    {
        Show();
    }

    public void CloseVsFriendsPanel()
    {
        if (isBusy) return;

        Hide();
    }

    public async void CreateGame()
    {
        if (isBusy) return;

        isBusy = true;
        createGameBtn.interactable = false;
        await HostSingleton.Instance.GameManager.StartHostAsync();
        //createGameBtn.interactable = true;
        isBusy = false;
    }

    public async void JoinGame()
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
        isBusy = false;
    }

    public async void QuickJoin()
    {
        if (isBusy) return;
        isBusy = true;
        await ClientSingleton.Instance.GameManager.QuickJoinLobbyAsync();
        isBusy = false;
    }

    public void CloseErrorLobbyCodePanel()
    {
        lobbyCodeErrorPanel.SetActive(false);
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
