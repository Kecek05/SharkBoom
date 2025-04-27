using QFSW.QC;
using Sortify;
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class LoadingPlayersUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject backgroundPlayersInfo;
    [SerializeField] private GameObject backgroundWaitingForPlayers;
    [SerializeField] private GameObject sharkPrefab;
    [SerializeField] private GameObject orcaPrefab;

    [BetterHeader("References Player 1")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player1PearlsText;
    [SerializeField] private Transform player1VisualSpawnpoint;

    [BetterHeader("References Player 2")]
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private TextMeshProUGUI player2PearlsText;
    [SerializeField] private Transform player2VisualSpawnpoint;

    

    private BaseGameStateManager gameStateManager;
    private BasePlayersPublicInfoManager basePlayerPublicInfoManager;

    private int updatedPlayersInfoOnClient = 0;

    private GameObject player1GameObject;
    private GameObject player2GameObject;

    public override void OnNetworkSpawn()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        basePlayerPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

        HidePlayersInfo();
        ShowWaitingForPlayers();

        gameStateManager.CurrentGameState.OnValueChanged += GameState_OnValueChanged;

        if(gameStateManager.CurrentGameState.Value != GameState.CalculatingResults && gameStateManager.CurrentGameState.Value != GameState.WaitingForPlayers && gameStateManager.CurrentGameState.Value != GameState.SpawningPlayers)
        {
            //Game already started, reconnected, hide all
            HidePlayersInfo();
            HideWaitingForPlayers();
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if(newValue == GameState.CalculatingResults)
        {
            //All Connected

        }
        else if(newValue == GameState.ShowingPlayersInfo)
        {
            //Show UI
            if (IsServer)
            {
                //Send to clients
                foreach (PlayerData playerData in NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas)
                {
                    Debug.Log($"GameState_OnValueChanged on Loading Players UI - Player Data: {playerData.userData.userAuthId} - Client Id: {playerData.clientId}");
                    UpdatePlayersInfoClientRpc(playerData.userData.userName, playerData.userData.userPearls, playerData.playableState);
                }
            }

            //ShowPlayersInfo();
            //HideWaitingForPlayers();
        } else if (newValue == GameState.GameStarted)
        {
            //Game Started
            HidePlayersInfo();
            HideWaitingForPlayers();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayersInfoClientRpc(FixedString32Bytes playerName, int playerPearls, PlayableState playableState)
    {
        updatedPlayersInfoOnClient++;

        Debug.Log($"Name: {playerName} Pearls: {playerPearls} Player Count: {updatedPlayersInfoOnClient}");

        //All clients listen to this
        switch(playableState)
        {
            case PlayableState.Player1Playing:
                player1NameText.text = playerName.ToString();
                player1PearlsText.text = playerPearls.ToString();
                player1GameObject = SpawnPlayerVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], player1VisualSpawnpoint);
                break;
            case PlayableState.Player2Playing:
                player2NameText.text = playerName.ToString();
                player2PearlsText.text = playerPearls.ToString();
                player2GameObject = SpawnPlayerVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], player2VisualSpawnpoint);
                break;
        }

        if(updatedPlayersInfoOnClient >= 2)
        {
            ShowPlayersInfo();
            HideWaitingForPlayers();
        }
    }

    private GameObject SpawnPlayerVisual(PlayerVisualType visualType, Transform spawnPoint)
    {
        GameObject prefabToSpawn = null;

        switch (visualType)
        {
            case PlayerVisualType.Shark:
                prefabToSpawn = sharkPrefab;
                break;
            case PlayerVisualType.Orca:
                prefabToSpawn = orcaPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            return Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

        return null;
    }

    [Command("hidePlayersInfo")]
    private void HidePlayersInfo()
    {
        backgroundPlayersInfo.SetActive(false);
        Destroy(player1GameObject);
        Destroy(player2GameObject);
    }

    [Command("showPlayersInfo")]
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
        gameStateManager.CurrentGameState.OnValueChanged -= GameState_OnValueChanged;
    }

}
