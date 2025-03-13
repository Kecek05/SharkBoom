using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button lobbiesButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button quickJoinButton;

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

        clientButton.onClick.AddListener(async() =>
        {
            clientButton.interactable = false;
            await ClientSingleton.Instance.GameManager.StartRelayClientAsync(lobbyCodeInputField.text);
            clientButton.interactable = true;
        });

        quickJoinButton.onClick.AddListener(async () =>
        {
            quickJoinButton.interactable = false;
            await ClientSingleton.Instance.GameManager.QuickJoinLobbyAsync();
            quickJoinButton.interactable = true;
        });
    }
}
