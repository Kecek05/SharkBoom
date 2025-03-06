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
    public PlayableState LocalplayableState => localPlayableState;

    private NetworkVariable<PlayableState> currentPlayableState = new(PlayableState.None);
    public NetworkVariable<PlayableState> CurrentPlayableState => currentPlayableState;

    private NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);

    private Dictionary<ulong, bool> playersReady = new();
    //private Dictionary<ulong, algo> playersRoundData = new();

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += GameState_OnValueChanged;

        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayableState = PlayableState.Player1Playing;
        }
        else
        {
            localPlayableState = PlayableState.Player2Playing;
        }
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
                }
                break;
            case GameState.GameEnded:
                break;
        }

        Debug.Log($"Game State Changed to: {newValue.ToString()}");
    }


    private async void DelayChangeState(GameState newGameState)
    {
        await Task.Delay(2000);
        SetGameStateRpc(newGameState);
    }

    [Command("gameFlowManager-randomizePlayersItems")]
    public async void RandomizePlayerItems()
    {
        await Task.Delay(3000); //Delay for player to connect
        //int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now
        int itemsInInventory = itemsListSO.allItemsSOList.Count; //all items

        for (int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(0, itemsListSO.allItemsSOList.Count);

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {
                playerInventory.SetPlayerItems(randomItemSOIndex);
                Debug.Log($"Player: {playerInventory.gameObject.name}");
            }
        }

        SetGameStateRpc(GameState.GameStarted);
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

}
