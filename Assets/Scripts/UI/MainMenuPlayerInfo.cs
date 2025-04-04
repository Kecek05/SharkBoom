using TMPro;
using UnityEngine;

public class MainMenuPlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerPearlsText;

    void Start()
    {
        Save.OnPlayerPearlsLoaded += HandlePearlsChanged;
        Save.OnPlayerPearlsChanged += HandlePearlsChanged;


        playerNameText.text = ClientSingleton.Instance.GameManager.UserData.userName;

        UpdatePlayerValues();
    }

    private void HandlePearlsChanged()
    {
        UpdatePlayerValues();
    }

    private void UpdatePlayerValues()
    {
        playerPearlsText.text = ClientSingleton.Instance.GameManager.UserData.userPearls.ToString();
    }

    private void OnDestroy()
    {
        Save.OnPlayerPearlsLoaded -= HandlePearlsChanged;
        Save.OnPlayerPearlsChanged -= HandlePearlsChanged;
    }
}
