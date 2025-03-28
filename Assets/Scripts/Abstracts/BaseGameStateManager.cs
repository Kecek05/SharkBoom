using System;
using Unity.Netcode;

public abstract class BaseGameStateManager : NetworkBehaviour
{
    //Events

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public event Action OnLostConnectionInHost;

    /// <summary>
    /// Called when the server should be closed.
    /// </summary>
    public event Action OnCanCloseServer;


    //Variables
    protected int delayClosePlayersInfo;
    protected NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);


    //Publics

    public int DelayClosePlayersInfo => delayClosePlayersInfo;
    public NetworkVariable<GameState> CurrentGameState => gameState;

    //Methods



    /// <summary>
    /// Game timer ended.
    /// </summary>
    public abstract void HandleOnGameTimerEnd();

    /// <summary>
    /// Player spawned in game.
    /// </summary>
    /// <param name="playerCount"></param>
    public abstract void HandleOnPlayerSpawned(int playerCount);

    /// <summary>
    /// Game state changed.
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    public abstract void HandleOnGameStateValueChanged(GameState newValue);

    public abstract void HandleOnGameOver();

    /// <summary>
    /// Trigger event to close the server
    /// </summary>
    protected void TriggerCanCloseServer() => OnCanCloseServer?.Invoke();

    protected void TriggerOnLostConnectionInHost() => OnLostConnectionInHost?.Invoke();



    /// <summary>
    /// Trigger Change the game state on server with a optional delay.
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="delayToChange"></param>
    public abstract void ChangeGameState(GameState gameState, int delayToChange = 0);

    /// <summary>
    /// Set the game state on server.
    /// </summary>
    /// <param name="newState"></param>
    protected abstract void SetGameState(GameState newState);

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public abstract void ConnectionLostHostAndClient();

}

/// <summary>
/// Related to game flow, use PlayableState to player relayed states.
/// </summary>
public enum GameState
{
    None,
    WaitingForPlayers, //Waiting for players to connect
    SpawningPlayers, //Spawning Players
    CalculatingResults, //Calculating Results
    ShowingPlayersInfo, // All players connected, and Spawned, Showing Players Info
    GameStarted, //Game Started
    GameEnded, //Game Over
}
