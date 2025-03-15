using Sortify;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LoadingPlayersUI : NetworkBehaviour
{
    [BetterHeader("References Player 1")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player1PearlsText;

    [BetterHeader("References Player 2")]
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private TextMeshProUGUI player2PearlsText;


    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {

        }
    }


    private void UpdatePlayer1UI(UserData userData)
    {
        player1NameText.text = userData.userName;
        player1PearlsText.text = userData.userPearls.ToString();
    }

    private void UpdatePlayer2UI(UserData userData)
    {
        player2NameText.text = userData.userName;
        player2PearlsText.text = userData.userPearls.ToString();
    }


}
