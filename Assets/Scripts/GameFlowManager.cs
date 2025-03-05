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
        WaitingForPlayers, //Waiting for players to connect
        GameStarted, //all players connected
        RoundPreparing, //waiting for all players to be ready
        RoundStarted, // all players ready
        RoundEnded,
        GameEnded, //Game Over
    }

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
            case GameState.RoundPreparing:
                OnRoundPreparing?.Invoke();
                break;
            case GameState.RoundStarted:
                OnRoundStarted?.Invoke();
                if (IsServer)
                {
                    DelayChangeState(GameState.RoundEnded);
                }
                break;
            case GameState.RoundEnded: //for some reason, client doesnt lisen to OnRoundEnd
                OnRoundEnd?.Invoke();

                if(IsServer)
                {
                    DelayChangeState(GameState.RoundPreparing);
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

        SetGameStateRpc(GameState.RoundPreparing);
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
            SetGameStateRpc(GameState.RoundStarted);
            playersReady.Clear(); //Clear for next round
        }
    }

    [Rpc(SendTo.Server)]
    public void SetGameStateRpc(GameState newState)
    {
        gameState.Value = newState;
    }

}
