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
    public static event Action OnLocalPlayableStateChanged;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;

    [BetterHeader("Settings")]
    [Unit("ms")][SerializeField] private int delayBetweenTurns = 3000;
    [Unit("ms")][SerializeField] private int delayClosePlayersInfo = 3000;

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


    /// <summary>
    /// Set the localPlayable and localPlayed states to the GameFlowManager, Only Owner.
    /// </summary>
    /// <param name="playingState"> Playing State</param>
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

        OnLocalPlayableStateChanged?.Invoke();
    }


    /// <summary>
    /// Called to change the player's turn.
    /// </summary>
    /// <param name="playerPlayingState"> Playing State</param>
    [Rpc(SendTo.Server)]
    public void PlayerPlayedServerRpc(PlayableState playerPlayingState)
    {
        if (playerPlayingState == PlayableState.Player1Playing)
        {
            //recived item callback from player 1
            currentPlayableState.Value = PlayableState.Player1Played;

            DelayChangeTurns(PlayableState.Player2Playing);
        }
        else if (playerPlayingState == PlayableState.Player2Playing)
        {
            //recived item callback from player 2
            currentPlayableState.Value = PlayableState.Player2Played;

            DelayChangeTurns(PlayableState.Player1Playing);
        }
    }


    /// <summary>
    /// Called when the player jumped.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    [Rpc(SendTo.Server)]
    public void PlayerJumpedServerRpc(PlayableState playableState)
    {
        PlayerJumpedClientRpc(playableState);
    }

    /// <summary>
    /// Tell the owner that the player jumped.
    /// </summary>
    /// <param name="playableState"> Owner Playing State</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerJumpedClientRpc(PlayableState playableState)
    {
        if (localPlayableState == playableState)
        {
            //if the jump item is the same as the player playing, owner jumped
            OnMyTurnJumped?.Invoke();
        }
    }

    /// <summary>
    /// Used to trigger local events when the player can play or not.
    /// </summary>
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
                    ChangeGameState(GameState.GameStarted, delayClosePlayersInfo); //Show Players Info delay
                }
                break;
            case GameState.GameStarted:
                if (IsServer)
                {

                    CurrentPlayableState.Value = PlayableState.Player1Playing; //DEBUG

                    //int randomStartPlayer = UnityEngine.Random.Range(0, 2);
                    //currentPlayableState.Value = randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing;
                }
                break;
            case GameState.GameEnded:
                break;
        }
    }


    /// <summary>
    /// Call this to change the player's turn after a delay.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    private async void DelayChangeTurns(PlayableState playableState)
    {
        await Task.Delay(delayBetweenTurns);
        SetPlayableStateServerRpc(playableState);
    }

    /// <summary>
    /// Call this to change the Game State, delay its optional.
    /// </summary>
    /// <param name="gameState"> Game State</param>
    /// <param name="delayToChange"> Delay in ms</param>
    public async void ChangeGameState(GameState gameState, int delayToChange = 0) //0ms default
    {
        await Task.Delay(delayToChange);
        SetGameStateServerRpc(gameState);
    }


    /// <summary>
    /// Call this to randomize and give items to players
    /// </summary>
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


    [Rpc(SendTo.Server)]
    private void SetGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }


    [Rpc(SendTo.Server)]
    private void SetPlayableStateServerRpc(PlayableState newState)
    {
        currentPlayableState.Value = newState;
    }

    /// <summary>
    /// Get a random Spawnpoint from the list and remove it from the list.
    /// </summary>
    /// <returns></returns>
    public Transform GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[UnityEngine.Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint;
    }


    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= GameState_OnValueChanged;

        if (IsClient)
        {
            currentPlayableState.OnValueChanged -= CurrentPlayableState_OnValueChanged;
        }
    }

}

/// <summary>
/// Related to game flow, use PlayableState to player relayed states.
/// </summary>
public enum GameState
{
    None,
    WaitingForPlayers, //Waiting for players to connect
    WaitingToStart, // All players connected, and Spawned
    GameStarted, //Game Started
    GameEnded, //Game Over
}


