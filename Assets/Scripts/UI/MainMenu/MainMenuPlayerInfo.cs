using TMPro;
using UnityEngine;

public class MainMenuPlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerPearlsText;

    void Start()
    {
        Save.OnPlayerPearlsChanged += HandlePearlsChanged;


        playerNameText.text = ClientSingleton.Instance.GameManager.UserData.userName;

        UpdatePlayerValues();
    }

    private void HandlePearlsChanged()
    {
        UpdatePlayerValues();
    }

    private async void UpdatePlayerValues()
    {

        int pearls = await Save.LoadPlayerPearls(ClientSingleton.Instance.GameManager.UserData.userAuthId);
        playerPearlsText.text = pearls.ToString();
    }

    private void OnDestroy()
    {
        Save.OnPlayerPearlsChanged -= HandlePearlsChanged;
    }
}
