using QFSW.QC;
using Sortify;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public event Action OnGameOver;

    [BetterHeader("Settings")]
    [Tooltip("in ms")][SerializeField] private int delayClosePlayersInfo = 3000;


    private NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);
    private NetworkVariable<PlayableState> losePlayableState = new(PlayableState.None);

    private bool gameOver = false;

    //Publics
    public NetworkVariable<PlayableState> LosePlayableState => losePlayableState;
    public bool GameOver => gameOver;
    public NetworkVariable<GameState> CurrentGameState => gameState;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            gameState.OnValueChanged += GameState_OnValueChanged;
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
                GameFlowManager.Instance.RandomizePlayerItems();
                ChangeGameState(GameState.GameStarted, delayClosePlayersInfo); //Show Players Info delay
                break;
            case GameState.GameStarted:
                GameFlowManager.Instance.TurnManager.StartGame();
                break;
            case GameState.GameEnded:
                gameOver = true;
                Debug.Log("Game Over!");
                OnGameOver?.Invoke();

                GameOverClientRpc();
                break;
        }

        Debug.Log($"Game State Changed to: {newValue}");
    }


    [Rpc(SendTo.NotServer)]
    private void GameOverClientRpc()
    {
        //Dont send to Host, already did on GameState_OnValueChanged

        gameOver = true;
        Debug.Log("Game Over!");
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// The lose player call this to know ho loses and ho wins. Server only
    /// </summary>
    /// <param name="playerLosedPlayableState"> Playing State</param>
    public void LoseGame(PlayableState playerLosedPlayableState)
    {
        losePlayableState.Value = playerLosedPlayableState;
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
    [Command("gameFlowManager-ChangeGameState")]
    private void SetGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            gameState.OnValueChanged -= GameState_OnValueChanged;
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
