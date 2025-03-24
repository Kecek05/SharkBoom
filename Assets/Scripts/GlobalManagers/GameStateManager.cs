using QFSW.QC;
using Sortify;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public event Action OnGameOver;
    public event Action OnWin;
    public event Action OnLose;

    [BetterHeader("Settings")]
    [Tooltip("in ms")][SerializeField] private int delayClosePlayersInfo = 3000;


    private NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);
    private NetworkVariable<PlayableState> losedPlayer = new(PlayableState.None);

    private bool gameOver = false;

    //Publics
    public NetworkVariable<PlayableState> LosedPlayer => losedPlayer;
    public bool GameOver => gameOver;
    public NetworkVariable<GameState> CurrentGameState => gameState;



    public override void OnNetworkSpawn()
    {
        losedPlayer.OnValueChanged += LosedPlayer_OnvalueChanged;
        gameState.OnValueChanged += GameState_OnValueChanged;

        if (IsServer)
        {
            PlayerHealth.OnPlayerDie += LoseGame;
            PlayerSpawner.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
        }
    }

    private void PlayerSpawner_OnPlayerSpawned(int playerCount)
    {
        if (playerCount == 2)
        {
            ChangeGameState(GameState.ShowingPlayersInfo); // All players Spawned, waiting to start
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        //Only Server

        switch (newValue)
        {
            case GameState.WaitingForPlayers:
                break;
            case GameState.SpawningPlayers:
                //All players connected
                Debug.Log("Start Spawning Players");

                break;
            case GameState.ShowingPlayersInfo:

                if(IsClient)
                    CalculatePearlsManager.CalculatePossibleResults();

                if(IsServer)
                {
                    GameFlowManager.Instance.RandomizePlayerItems();
                    ChangeGameState(GameState.GameStarted, delayClosePlayersInfo); //Show Players Info delay
                }
                break;
            case GameState.GameStarted:
                if(IsServer)
                    GameFlowManager.Instance.TurnManager.StartGame();
                break;
            case GameState.GameEnded:
                if(IsClient)
                {
                    GameOverAsync();
                }
                break;
        }

        Debug.Log($"Game State Changed to: {newValue}");
    }

    private void LosedPlayer_OnvalueChanged(PlayableState previousValue, PlayableState newValue)
    {
        gameOver = true;
        OnGameOver?.Invoke();

        if(IsServer)
            ChangeGameState(GameState.GameEnded);
        
    }

    private async void GameOverAsync()
    {
        if (losedPlayer.Value == GameFlowManager.Instance.TurnManager.LocalPlayableState)
        {
            //Change pearls, than lose
            await CalculatePearlsManager.TriggerChangePearls(false);
            OnLose?.Invoke();
        }
        else if (losedPlayer.Value == PlayableState.None)
        {
            //Tie, lose
            //Change pearls, than lose
            await CalculatePearlsManager.TriggerChangePearls(false);
            OnLose?.Invoke();
        }
        else
        {
            //Change pearls, than win
            await CalculatePearlsManager.TriggerChangePearls(true);
            OnWin?.Invoke();
        }
    }

    /// <summary>
    /// Call this to know who loses and who wins. Server only.
    /// </summary>
    /// <param name="playerLosedPlayableState"> Playing State of the player who loses</param>
    public void LoseGame(PlayableState playerLosedPlayableState)
    {
        losedPlayer.Value = playerLosedPlayableState;
    }

    /// <summary>
    /// Call this to change the Game State, delay its optional.
    /// </summary>
    /// <param name="gameState"> Game State</param>
    /// <param name="delayToChange"> Delay in ms</param>
    public async void ChangeGameState(GameState gameState, int delayToChange = 0) //0ms default
    {
        if (gameOver) return;

        await Task.Delay(delayToChange);
        SetGameStateServerRpc(gameState);
    }

    [Rpc(SendTo.Server)]
    private void SetGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }

    public override void OnNetworkDespawn()
    {
        losedPlayer.OnValueChanged -= LosedPlayer_OnvalueChanged;
        gameState.OnValueChanged -= GameState_OnValueChanged;

        if (IsServer)
        {
            PlayerHealth.OnPlayerDie -= LoseGame;
            PlayerSpawner.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
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
    SpawningPlayers, //Spawning Players
    ShowingPlayersInfo, // All players connected, and Spawned, Showing Players Info
    GameStarted, //Game Started
    GameEnded, //Game Over
}
