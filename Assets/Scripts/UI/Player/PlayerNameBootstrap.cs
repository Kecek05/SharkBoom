using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameBootstrap : MonoBehaviour
{
    private const int MAX_CHARACTERS = 12;
    private const int MIN_CHARACTERS = 5;

    [SerializeField] private GameObject renameScreen;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject errorPanelName;

    private void Awake()
    {
        errorPanelName.SetActive(false);
        renameScreen.SetActive(false);
        loadingText.enabled = false;
    }

    private void Start()
    {
        if (ClientSingleton.Instance.GameManager.UserData.userName == "")
        {
            renameScreen.SetActive(true);
        }
        else
        {
            loadingText.enabled = true;
            Loader.LoadNoLoadingScreen(Loader.Scene.GameTutorial);
        }
    }

    public async void ConfirmName()
    {
        string playerName = playerNameInputField.text;

        if (string.IsNullOrEmpty(playerName) || playerName.Length > MAX_CHARACTERS || playerName.Length < MIN_CHARACTERS)
        {
            errorPanelName.SetActive(true); 
            return;
        }
        playerNameInputField.interactable = false;
        confirmButton.interactable = false;

        await Save.SavePlayerName(AuthenticationService.Instance.PlayerId, playerNameInputField.text);

        ClientSingleton.Instance.GameManager.UserData.SetPlayerName(await Save.LoadPlayerName(AuthenticationService.Instance.PlayerId));
        renameScreen.SetActive(false);
        loadingText.enabled = true;
        Loader.LoadNoLoadingScreen(Loader.Scene.GameTutorial);
    }

    public void CloseErrorPanel()
    {
        errorPanelName.SetActive(false);
    }
}
