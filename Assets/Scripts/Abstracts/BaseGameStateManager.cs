using System;
using Unity.Netcode;

public abstract class BaseGameStateManager : NetworkBehaviour
{
    //Events

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public event Action OnLostConnectionInHost;


    //Variables
    protected const int DELAY_CLOSE_PLAYERSINFO = 3000; //in ms
    protected const int DELAY_STARTGAME = 3000; //in ms
    protected NetworkVariable<GameState> gameState = new(GameState.WaitingForPlayers);
    protected int clientsGainedOwnership = 0;

    //Publics
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


    protected void TriggerOnLostConnectionInHost() => OnLostConnectionInHost?.Invoke();



    /// <summary>
    /// Trigger Change the game state on server with a optional delay.
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="delayToChange"></param>
    public abstract void ChangeGameState(GameState gameState, int delayToChange = 0);

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public abstract void ConnectionLostHostAndClient();

    public abstract void HandeOnPlayerDie();

    public abstract void HandleOnPlayerGainOwnership(ulong clientId);

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
