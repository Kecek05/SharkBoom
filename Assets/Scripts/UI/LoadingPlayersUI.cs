using QFSW.QC;
using Sortify;
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Video;

public class LoadingPlayersUI : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private GameObject backgroundPlayersInfo;
    [SerializeField] private GameObject backgroundWaitingForPlayers;
    [SerializeField] private VideoClip sharkRenderR;
    [SerializeField] private VideoClip sharkRenderL;
    [SerializeField] private VideoClip orcaRenderR;
    [SerializeField] private VideoClip orcaRenderL;

    [BetterHeader("References Player 1")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player1PearlsText;
    [SerializeField] private VideoPlayer player1VideoPlayer;

    [BetterHeader("References Player 2")]
    [SerializeField] private TextMeshProUGUI player2NameText;
    [SerializeField] private TextMeshProUGUI player2PearlsText;
    [SerializeField] private VideoPlayer player2VideoPlayer;

    private const string TEXTANIMATOR_NAMETAG = "<name>";

    private BaseGameStateManager gameStateManager;
    private BasePlayersPublicInfoManager basePlayerPublicInfoManager;

    private int updatedPlayersInfoOnClient = 0;

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
                    BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
                    UpdatePlayerVisualTypeClientRpc(playerData.playableState, playersPublicInfoManager.GetPlayerVisualTypes()[playerData.playableState]);
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
    private void UpdatePlayerVisualTypeClientRpc(PlayableState playableState, PlayerVisualType playerVisualType)
    {
        BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
        playersPublicInfoManager.SetPlayerVisualType(playableState, playerVisualType);
        Debug.Log("UpdatePlayerVisualTypeClientRpc - PlayerVisualType: " + playerVisualType + " PlayableState: " + playableState + " ClientId: " + NetworkManager.LocalClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayersInfoClientRpc(FixedString32Bytes playerName, int playerPearls, PlayableState playableState)
    {
        updatedPlayersInfoOnClient++;

        //All clients listen to this
        switch(playableState)
        {
            case PlayableState.Player1Playing:
                player1NameText.text = $"{TEXTANIMATOR_NAMETAG}{playerName.ToString()}{TEXTANIMATOR_NAMETAG}";
                player1PearlsText.text = playerPearls.ToString();
                player1VideoPlayer.clip = SelectRenderVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], PlayableState.Player1Playing);
                break;
            case PlayableState.Player2Playing:
                player2NameText.text = $"{TEXTANIMATOR_NAMETAG}{playerName.ToString()}{TEXTANIMATOR_NAMETAG}";
                player2PearlsText.text = playerPearls.ToString();
                player2VideoPlayer.clip = SelectRenderVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], PlayableState.Player2Playing);
                break;
        }

        if(updatedPlayersInfoOnClient >= 2)
        {
            ShowPlayersInfo();
            HideWaitingForPlayers();
        }
    }

    private VideoClip SelectRenderVisual(PlayerVisualType visualType, PlayableState playableState)
    {
        VideoClip videoClipSelected = null;

        switch (playableState)
        {
            case PlayableState.Player1Playing:
                switch (visualType)
                {
                    case PlayerVisualType.Shark:
                        return sharkRenderL;
                    case PlayerVisualType.Orca:
                        return orcaRenderL;
                }
                break;

            case PlayableState.Player2Playing:
                switch (visualType)
                {
                    case PlayerVisualType.Shark:
                        return sharkRenderR;
                    case PlayerVisualType.Orca:
                        return orcaRenderR;
                }
                break;
        }

        return videoClipSelected;
    }

    [Command("hidePlayersInfo")]
    private void HidePlayersInfo()
    {
        backgroundPlayersInfo.SetActive(false);
        player1VideoPlayer.Stop();
        player2VideoPlayer.Stop();
    }

    [Command("showPlayersInfo")]
    private void ShowPlayersInfo()
    {
        backgroundPlayersInfo.SetActive(true);
        player1VideoPlayer.Play();
        player2VideoPlayer.Play();
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
