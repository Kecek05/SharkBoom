using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameHandler : MonoBehaviour
{
    [SerializeField] private GameObject renameScreen;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_InputField playerNameInputField;

    private async void Awake()
    {
        renameScreen.SetActive(false);

        confirmButton.onClick.AddListener(async () =>
        {
            playerNameInputField.interactable = false;
            confirmButton.interactable = false;

            await Save.SavePlayerName(AuthenticationService.Instance.PlayerId, playerNameInputField.text);

            ClientSingleton.Instance.GameManager.UserData.SetPlayerName(await Save.LoadPlayerName(AuthenticationService.Instance.PlayerId));
            renameScreen.SetActive(false);

        });

        if (ClientSingleton.Instance.GameManager.UserData.userName == "")
        {
            renameScreen.SetActive(true);
        }
    }
}
