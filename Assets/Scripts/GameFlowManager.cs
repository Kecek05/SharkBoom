using QFSW.QC;
using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameFlowManager : NetworkBehaviour
{
    private static GameFlowManager instance;
    public static GameFlowManager Instance => instance;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;

    public static event Action OnRoundTrigger;

    private enum GameState
    {
        WaitingForPlayers, //Waiting for players to connect
        GameStarted, //all players connected
        WaitingForPlayersReady, //waiting for all players to be ready
        PlayersReady, // all players ready
        RoundGoing,
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
        OnRoundTrigger?.Invoke();

        Debug.Log($"Game State Changed to: {newValue.ToString()}");
    }

    [Command("gameFlowManager-randomizePlayersItems")]
    public void RandomizePlayerItems()
    {
        int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now

        for(int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(0, itemsListSO.allItemsSOList.Count);

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

    [Command("gameFlowManager-setPlayerReady")]
    [Rpc(SendTo.Server)]
    public void SetPlayerReadyRpc(ServerRpcParams serverRpcParams = default)
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
            gameState.Value = GameState.PlayersReady;
            playersReady.Clear(); //Clear for next round
            Debug.Log("All Players Ready!");
        }
    }

}
