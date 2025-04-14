using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject renameScreen;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_InputField playerNameInputField;

    private async void Awake()
    {
        renameScreen.SetActive(false);
        confirmButton.interactable = false;

        confirmButton.onClick.AddListener(async () =>
        {
            playerNameInputField.interactable = false;
            confirmButton.interactable = false;

            await Save.SavePlayerName(AuthenticationService.Instance.PlayerId, playerNameInputField.text);

            ClientSingleton.Instance.GameManager.UserData.SetPlayerName(await Save.LoadPlayerName(AuthenticationService.Instance.PlayerId));
            renameScreen.SetActive(false);

            Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
        });

        playerNameInputField.onValueChanged.AddListener(HandlePlayerName);
    }

    private void Start()
    {
        if (ClientSingleton.Instance.GameManager.UserData.userName == "")
        {
            renameScreen.SetActive(true);
        } else
        {
            Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
        }
    }

    private void HandlePlayerName(string playerName)
    {
        if(string.IsNullOrEmpty(playerName) || playerName.Length > 15 || playerName.Length < 5)
        {
            confirmButton.interactable = false;
        }
        else
        {
            confirmButton.interactable = true;
        }
    }
}
