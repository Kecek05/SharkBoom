using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject renameScreen;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject errorPanelName;

    private async void Awake()
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
            Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
        }
    }

    public async void ConfirmName()
    {
        string playerName = playerNameInputField.text;

        if (string.IsNullOrEmpty(playerName) || playerName.Length > 15 || playerName.Length < 5)
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
        Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
    }

    public void CloseErrorPanel()
    {
        errorPanelName.SetActive(false);
    }
}
