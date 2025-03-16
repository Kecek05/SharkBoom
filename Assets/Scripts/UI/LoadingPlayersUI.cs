using Sortify;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LoadingPlayersUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject backgroundPlayersInfo;
    [SerializeField] private GameObject backgroundWaitingForPlayers;

    [BetterHeader("References Player 1")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player1PearlsText;

    [BetterHeader("References Player 2")]
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private TextMeshProUGUI player2PearlsText;

    public override void OnNetworkSpawn()
    {
        HidePlayersInfo();
        ShowWaitingForPlayers();


        if(IsClient)
        {
            GameFlowManager.Instance.CurrentGameState.OnValueChanged += GameState_OnValueChanged;
        }

        if(IsServer)
        {
            Player.OnPlayerSpawned += Player_OnPlayerSpawned;
        }

        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GetUserDataRpc(clientId);
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if(newValue == GameState.WaitingToStart)
        {
            //All Connected
            ShowPlayersInfo();
            HideWaitingForPlayers();
        } else if (newValue == GameState.GameStarted)
        {
            //Game Started
            HidePlayersInfo();
        }
        Debug.Log(newValue);
    }


    private void Player_OnPlayerSpawned(Player player)
    {
        
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

    [Rpc(SendTo.Server)]
    public void GetUserDataRpc(ulong clientId) //Call server to retrive client Id
    {
        if(IsHost)
        {
            //Call from HostSingleton

        } else
        {
            //Call from ServerSingleton
        }
    }

    private void HidePlayersInfo()
    {
        backgroundPlayersInfo.SetActive(false);
    }

    private void ShowPlayersInfo()
    {
        backgroundPlayersInfo.SetActive(true);
    }

    private void HideWaitingForPlayers()
    {
        backgroundWaitingForPlayers.SetActive(false);
    }

    private void ShowWaitingForPlayers()
    {
        backgroundWaitingForPlayers.SetActive(true);
    }


    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            GameFlowManager.Instance.CurrentGameState.OnValueChanged -= GameState_OnValueChanged;
        }

        if (IsServer)
        {
            Player.OnPlayerSpawned -= Player_OnPlayerSpawned;
        }
    }

}
