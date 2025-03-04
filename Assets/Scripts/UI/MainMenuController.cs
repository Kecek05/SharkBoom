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

    private async void Awake()
    {
        hostButton.onClick.AddListener(async () =>
        {
            hostButton.interactable = false;
            HostSingleton.Instance.Star
        });
    }
}
