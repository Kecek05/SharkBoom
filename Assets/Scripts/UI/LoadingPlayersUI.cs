using Sortify;
using TMPro;
using Unity.Collections;
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
            PlayerThrower.OnPlayerSpawned += Player_OnPlayerSpawned;
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {

        if(newValue == GameState.WaitingToStart)
        {
            //All Connected and Spawned

            ShowPlayersInfo();
            HideWaitingForPlayers();
        } else if (newValue == GameState.GameStarted)
        {
            //Game Started
            HidePlayersInfo();
        }
    }


    private void Player_OnPlayerSpawned(PlayerThrower player)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count == 2) //Only update if the second player is spawned
            PlayerSpawnedServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void PlayerSpawnedServerRpc()
    {
        //Send to server to send to all clients

        foreach(ulong connectedClientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(IsHost)
            {
                //Host Singleton
                UserData playerUserData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(connectedClientId);

                PlayerSpawnedClientRpc(playerUserData.userName, playerUserData.userPearls, connectedClientId);
            } else
            {
                //Server Singleton
                //UserData playerUserData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(connectedClientId);

                //PlayerSpawnedClientRpc(playerUserData.userName, playerUserData.userPearls, connectedClientId);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerSpawnedClientRpc(FixedString32Bytes playerName, int playerPearls, ulong clientId)
    {
        //All clients listen to this
        if (clientId == 0)
        {
            //Player 1
            player1NameText.text = playerName.Value.ToString();
            player1PearlsText.text = playerPearls.ToString();
        }
        else
        {
            //Player 2
            player2NameText.text = playerName.Value.ToString();
            player2PearlsText.text = playerPearls.ToString();
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
            PlayerThrower.OnPlayerSpawned -= Player_OnPlayerSpawned;
        }
    }

}
