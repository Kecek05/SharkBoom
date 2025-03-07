using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{
    private static GameFlowManager instance;
    public static GameFlowManager Instance => instance;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;


    public static event Action OnRoundPreparing; //Preparing fase, players can play
    public static event Action OnRoundStarted; // Round started, players just watch
    public static event Action OnRoundEnd; // Round finished, future implementations
    public static event Action OnMyTurnStarted; //local player can play
    public static event Action OnMyTurnEnded;

    public enum GameState
    {
        None,
        WaitingForPlayers, //Waiting for players to connect
        GameStarted, //all players connected
        GameEnded, //Game Over
    }

    public enum PlayableState
    {
        None,
        Player1Playing, //Player 1 Can Play
        Player1Played, //Player 1 Cant Play
        Player2Playing, //Player 2 Can Play
        Player2Played, //Player 2 Cant Play
    }

    private PlayableState localPlayableState = new();
    private PlayableState localPlayedState = new();

    private NetworkVariable<PlayableState> currentPlayableState = new(PlayableState.None);

    private NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);

    private Dictionary<ulong, bool> playersReady = new();
    //private Dictionary<ulong, algo> playersRoundData = new();

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

        if(IsClient)
        {
            currentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;

            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                localPlayableState = PlayableState.Player1Playing;
                localPlayedState = PlayableState.Player1Played;
            }
            else
            {
                localPlayableState = PlayableState.Player2Playing;
                localPlayedState = PlayableState.Player2Played;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerPlayedRpc(PlayableState playerPlayingState)
    {
        if(playerPlayingState == PlayableState.Player1Playing)
        {
            currentPlayableState.Value = PlayableState.Player1Played;

            DelayChangeState(PlayableState.Player2Playing);
        }
        else if (playerPlayingState == PlayableState.Player2Playing)
        {
            currentPlayableState.Value = PlayableState.Player2Played;

            DelayChangeState(PlayableState.Player1Playing);
        }
    }
    

    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(newValue == localPlayableState)
        {
            //Local Player can play
            OnMyTurnStarted?.Invoke();
        } else if (newValue == localPlayedState)
        {
            //Local Player cant play
            OnMyTurnEnded?.Invoke();
        }

        Debug.Log($"Current Playable State Changed to: {newValue.ToString()}");
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        switch(newValue)
        {
            case GameState.WaitingForPlayers:
                break;
            case GameState.GameStarted:
                if(IsServer)
                {
                    RandomizePlayerItems();

                    int randomStartPlayer = UnityEngine.Random.Range(0, 2);
                    CurrentPlayableState.Value = PlayableState.Player2Playing;
                    //currentPlayableState.Value = randomStartPlayer == 0 ? PlayableState.Player1Playing : PlayableState.Player2Playing;
                }
                break;
            case GameState.GameEnded:
                break;
        }

        Debug.Log($"Game State Changed to: {newValue.ToString()}");
    }


    private async void DelayChangeState(PlayableState playableState)
    {
        await Task.Delay(2000);
        SetPlayableStateRpc(playableState);
    }

    [Command("gameFlowManager-randomizePlayersItems")]
    public async void RandomizePlayerItems()
    {
        await Task.Delay(3000); //Delay for player to connect
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
                Debug.Log($"Player: {playerInventory.gameObject.name}");
            }
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[UnityEngine.Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint.position;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playersReady[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playersReady.ContainsKey(clientID) || !playersReady[clientID])
            {
                //This player is not ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            //SetGameStateRpc(GameState.RoundStarted);
            playersReady.Clear(); //Clear for next round
        }
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
