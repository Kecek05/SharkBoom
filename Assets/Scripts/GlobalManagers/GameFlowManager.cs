using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{
    private static GameFlowManager instance;
    public static GameFlowManager Instance => instance;




    public static event Action OnMyTurnStarted; //local player can play
    public static event Action OnMyTurnEnded;
    public static event Action OnMyTurnJumped;


    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;


    private PlayableState localPlayableState = new();
    private PlayableState localPlayedState = new();

    private NetworkVariable<PlayableState> currentPlayableState = new(PlayableState.None);

    private NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);

    public NetworkVariable<GameState> CurrentGameState => gameState;
    public NetworkVariable<PlayableState> CurrentPlayableState => currentPlayableState;
    public PlayableState LocalplayableState => localPlayableState;
    public PlayableState LocalplayedState => localPlayedState;

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += GameState_OnValueChanged;

        if (IsClient)
        {
            currentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;
        }
    }

    public void SetLocalStates(PlayableState playingState)
    {
        switch(playingState)
        {
            case PlayableState.Player1Playing:
                localPlayableState = PlayableState.Player1Playing;
                localPlayedState = PlayableState.Player1Played;
                break;
            case PlayableState.Player2Playing:
                localPlayableState = PlayableState.Player2Playing;
                localPlayedState = PlayableState.Player2Played;
                break;
        }

    }

    [Rpc(SendTo.Server)]
    public void PlayerPlayedRpc(PlayableState playerPlayingState)
    {
        if (playerPlayingState == PlayableState.Player1Playing)
        {
            //recived item callback from player 1
            currentPlayableState.Value = PlayableState.Player1Played;

            DelayChangePlayableState(PlayableState.Player2Playing);
        }
        else if (playerPlayingState == PlayableState.Player2Playing)
        {
            //recived item callback from player 2
            currentPlayableState.Value = PlayableState.Player2Played;

            DelayChangePlayableState(PlayableState.Player1Playing);
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerJumpedServerRpc(PlayableState playableState)
    {
        PlayerJumpedClientRpc(playableState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerJumpedClientRpc(PlayableState playableState)
    {
        if (localPlayableState == playableState)
        {
            //if the jump item is the same as the player playing, owner jumped
            OnMyTurnJumped?.Invoke();
        }
    }

    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if (newValue == localPlayableState)
        {
            //Local Player can play
            OnMyTurnStarted?.Invoke();
        }
        else if (newValue == localPlayedState)
        {
            //Local Player cant play
            OnMyTurnEnded?.Invoke();
        }

    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingForPlayers:
                break;
            case GameState.WaitingToStart: //All connected, showing Players Info

                if(IsServer)
                {
                    RandomizePlayerItems();
                    DelayChangeGameState(GameState.GameStarted); //Show delay
                }
                break;
            case GameState.GameStarted:
                if (IsServer)
                {

                    CurrentPlayableState.Value = PlayableState.Player1Playing; //DEBUG
                    int randomStartPlayer = UnityEngine.Random.Range(0, 2);
                    //currentPlayableState.Value = randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing;
                }
                break;
            case GameState.GameEnded:
                break;
        }

        Debug.Log($"Game State Changed to: {newValue.ToString()}");
    }

    private async void DelayChangePlayableState(PlayableState playableState)
    {
        await Task.Delay(3000);
        SetPlayableStateRpc(playableState);
    }

    private async void DelayChangeGameState(GameState gameState)
    {
        await Task.Delay(3000);
        SetGameStateRpc(gameState);
    }

    [Command("gameFlowManager-randomizePlayersItems")]
    public void RandomizePlayerItems()
    {
        //int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now
        int itemsInInventory = itemsListSO.allItemsSOList.Count; //all items

        //Add Jump item first
        foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            playerInventory.SetPlayerItems(0);
        }

        for (int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(1, itemsListSO.allItemsSOList.Count); //Start from index 1,index 0 is jump

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {

                playerInventory.SetPlayerItems(randomItemSOIndex);
            }
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[UnityEngine.Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint;
    }

    [Rpc(SendTo.Server)]
    public void SetGameStateRpc(GameState newState)
    {
        gameState.Value = newState;
    }


    [Rpc(SendTo.Server)]
    public void SetPlayableStateRpc(PlayableState newState)
    {
        currentPlayableState.Value = newState;
    }
}

public enum GameState
{
    None,
    WaitingForPlayers, //Waiting for players to connect
    WaitingToStart, // All players connected, and Spawned
    GameStarted, //Game Started
    GameEnded, //Game Over
}


