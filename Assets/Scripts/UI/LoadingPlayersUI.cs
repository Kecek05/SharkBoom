using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

public class LoadingPlayersUI : NetworkBehaviour
{
    private readonly WaitForSeconds DELAY_CLOSE_PLAYERSINFO = new WaitForSeconds(3f);
    
    private const string TEXTANIMATOR_NAMETAG = "<name>";
    
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

    private bool alreadyShowedPlayersInfo = false;

    private BaseGameStateManager gameStateManager;
    private BasePlayersPublicInfoManager basePlayerPublicInfoManager;

    private int updatedPlayersInfoOnClient = 0;

    private bool hasPreparedPlayer1 = false;
    private bool hasPreparedPlayer2 = false;

    private void HandleOnPlayer1VideoPlayerPrepared(VideoPlayer source)
    {
        hasPreparedPlayer1 = true;
        Debug.Log($"Player 1 prepared: {hasPreparedPlayer1}");
    }

    private void HandleOnPlayer2VideoPlayerPrepared(VideoPlayer source)
    {
        hasPreparedPlayer2 = true;
        Debug.Log($"Player 2 prepared: {hasPreparedPlayer2}");
    }

    

    public override void OnNetworkSpawn()
    {
        hasPreparedPlayer1 = false;
        hasPreparedPlayer2 = false;

        player1VideoPlayer.prepareCompleted += HandleOnPlayer1VideoPlayerPrepared;
        player2VideoPlayer.prepareCompleted += HandleOnPlayer2VideoPlayerPrepared;

        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        basePlayerPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
        
        HidePlayersInfo();
        ShowWaitingForPlayers();

        gameStateManager.CurrentGameState.OnValueChanged += GameState_OnValueChanged;
        GameState_OnValueChanged(GameState.None, gameStateManager.CurrentGameState.Value);
        
        // if (gameStateManager.CurrentGameState.Value == GameState.ShowingPlayersInfo || gameStateManager.CurrentGameState.Value == GameState.GameStarted)
        // {
        //     //Game already started, reconnected
        //     if(IsClient)
        //         RequestDataToTheServerRpc();
        // }
        //
        // if(gameStateManager.CurrentGameState.Value != GameState.CalculatingResults && gameStateManager.CurrentGameState.Value != GameState.WaitingForPlayers && gameStateManager.CurrentGameState.Value != GameState.SpawningPlayers)
        // {
        //     // //Game already started, reconnected
        //     // if(IsClient)
        //     //     RequestDataToTheServerRpc();
        //     // HidePlayersInfo();
        //     // HideWaitingForPlayers();
        // }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestDataToTheServerRpc(ulong senderClientID)
    {
        // Debug.Log($"REQUESTING DATA TO THE SERVER LOADING PLAYERS UI - Sender ID: {senderClientID} - Players in Data: {NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas.Count}");
        StartCoroutine(WaitAllPlayersData(senderClientID));
    }

    private IEnumerator WaitAllPlayersData(ulong senderClientID)
    {
        while (NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas.Count < 2)
        {
            // Debug.Log($"WAITING FOR PLAYERS DATA - CURRENT: {NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas.Count}");
            yield return null;
        }
        // Debug.Log($"Server Recieved both players Data sending it to: {senderClientID}");
        //Send to clients
        foreach (PlayerData playerData in NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas)
        {
            // Debug.Log($"RequestDataToTheServerRpc on Loading Players UI - Player Data: {playerData.userData.userAuthId} - Client Id: {playerData.clientId} - Sender: {senderClientID}");
            UpdatePlayerVisualTypeClientRpc(playerData.playableState, basePlayerPublicInfoManager.GetPlayerVisualTypes()[playerData.playableState], senderClientID);
            UpdatePlayersInfoClientRpc(playerData.userData.userName, playerData.userData.userPearls, playerData.playableState, senderClientID);
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        // Debug.Log($"GameState_OnValueChanged - Previous Value: {previousValue} - New Value: {newValue} - {gameObject.name} - Is Owner: {IsOwner} - Client ID: {NetworkManager.LocalClientId}");
         if(newValue == GameState.ShowingPlayersInfo) 
         {
            //Show UI
            // if (IsServer)
            // {
            //     foreach (ulong connectedClientsId in NetworkManager.ConnectedClientsIds)
            //     {
            //         if(IsServer && !IsHost && connectedClientsId == 0) continue; //Not send the id 0 if is DS
            //         RequestDataToTheServerRpc(connectedClientsId);
            //     }
            // }
            
            // Debug.Log($"CLIENT REQUESTING DATA LOADING PLAYERS UI - {gameObject.name} - Is Owner: {IsOwner} - Client ID: {NetworkManager.LocalClientId} - Game State: {newValue}");
            RequestDataToTheServerRpc(NetworkManager.LocalClientId);
            alreadyShowedPlayersInfo = true;
            // PassPlayersDataToClients();

            //ShowPlayersInfo();
            //HideWaitingForPlayers();
         } 
         else if (newValue == GameState.GameStarted)
         {
            if (!alreadyShowedPlayersInfo)
            {
                // Debug.Log($"CLIENT REQUESTING DATA LOADING PLAYERS UI - {gameObject.name} - Is Owner: {IsOwner} - Client ID: {NetworkManager.LocalClientId} - Game State: {newValue}");
                RequestDataToTheServerRpc(NetworkManager.LocalClientId);
                alreadyShowedPlayersInfo = true;
            }
            //Game Started
            // HidePlayersInfo();
            // HideWaitingForPlayers();
         }
    }

    // private void PassPlayersDataToClients()
    // {
    //     if (IsServer)
    //     {
    //         // //Send to clients
    //         // foreach (PlayerData playerData in NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.PlayerDatas)
    //         // {
    //         //     Debug.Log($"GameState_OnValueChanged on Loading Players UI - Player Data: {playerData.userData.userAuthId} - Client Id: {playerData.clientId}");
    //         //     UpdatePlayerVisualTypeClientRpc(playerData.playableState, basePlayerPublicInfoManager.GetPlayerVisualTypes()[playerData.playableState]);
    //         //     UpdatePlayersInfoClientRpc(playerData.userData.userName, playerData.userData.userPearls, playerData.playableState);
    //         // }
    //     }
    // }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void UpdatePlayerVisualTypeClientRpc(PlayableState playableState, PlayerVisualType playerVisualType, ulong senderClientId)
    {
        //Only called to the client who requested it
        if(senderClientId != NetworkManager.LocalClientId) return;
        
        basePlayerPublicInfoManager.SetPlayerVisualType(playableState, playerVisualType);
        // Debug.Log("UpdatePlayerVisualTypeClientRpc - PlayerVisualType: " + playerVisualType + " PlayableState: " + playableState + " ClientId: " + NetworkManager.LocalClientId);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void UpdatePlayersInfoClientRpc(FixedString32Bytes playerName, int playerPearls, PlayableState playableState, ulong senderClientId)
    {
        //Only called to the client who requested it
        if(senderClientId != NetworkManager.LocalClientId) return;
        
        updatedPlayersInfoOnClient++;
        
        switch(playableState)
        {
            case PlayableState.Player1Playing:
                player1NameText.text = $"{TEXTANIMATOR_NAMETAG}{playerName.ToString()}{TEXTANIMATOR_NAMETAG}";
                player1PearlsText.text = playerPearls.ToString();
                player1VideoPlayer.clip = SelectRenderVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], PlayableState.Player1Playing);
                player1VideoPlayer.Prepare();
                break;
            case PlayableState.Player2Playing:
                player2NameText.text = $"{TEXTANIMATOR_NAMETAG}{playerName.ToString()}{TEXTANIMATOR_NAMETAG}";
                player2PearlsText.text = playerPearls.ToString();
                player2VideoPlayer.clip = SelectRenderVisual(basePlayerPublicInfoManager.GetPlayerVisualTypes()[playableState], PlayableState.Player2Playing);
                player2VideoPlayer.Prepare();
                break;
        }
    
        if(updatedPlayersInfoOnClient >= 2)
        {
            StartCoroutine(WaitUntilVideosPreparedAndShow());
        }
        
        // Debug.Log($"UpdatePlayersInfoClientRpc - Player Name: {playerName.ToString()} - Sender ID: {senderClientId} - Count: {updatedPlayersInfoOnClient}");
    }

    private IEnumerator WaitUntilVideosPreparedAndShow()
    {
        ShowPlayersInfo();

        while (!hasPreparedPlayer1 && !hasPreparedPlayer2)
            yield return null;

        HideWaitingForPlayers();

        yield return DELAY_CLOSE_PLAYERSINFO;
        HidePlayersInfo();
        HideWaitingForPlayers();
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
                        return sharkRenderR;
                    case PlayerVisualType.Orca:
                        return orcaRenderR;
                }
                break;

            case PlayableState.Player2Playing:
                switch (visualType)
                {
                    case PlayerVisualType.Shark:
                        return sharkRenderL;
                    case PlayerVisualType.Orca:
                        return orcaRenderL;
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
        player1VideoPlayer.prepareCompleted -= HandleOnPlayer1VideoPlayerPrepared;
        player2VideoPlayer.prepareCompleted -= HandleOnPlayer2VideoPlayerPrepared;
    }

}
