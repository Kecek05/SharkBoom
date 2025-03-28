using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseGameStateManager : NetworkBehaviour
{
    //Events
    public event Action<GameState> OnGameStateChange;

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

    public abstract override void OnNetworkSpawn();

    /// <summary>
    /// Game timer ended.
    /// </summary>
    protected abstract void HandleOnGameTimerEnd();

    /// <summary>
    /// Player spawned in game.
    /// </summary>
    /// <param name="playerCount"></param>
    protected abstract void HandleOnPlayerSpawned(int playerCount);

    /// <summary>
    /// Game state changed.
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    protected abstract void HandleOnGameStateValueChanged(GameState previousValue, GameState newValue);


    /// <summary>
    /// Trigger event to close the server
    /// </summary>
    [Rpc(SendTo.Server)]
    protected void TriggerCanCloseServerRpc()
    {
        OnCanCloseServer?.Invoke();
    }

    protected void TriggerOnGameStateChange()
    {
        OnGameStateChange?.Invoke(gameState.Value);
    }

    protected void TriggerOnLostConnectionInHost()
    {
        OnLostConnectionInHost?.Invoke();
    }



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
    [Rpc(SendTo.Server)]
    protected abstract void SetGameStateServerRpc(GameState newState);

    /// <summary>
    /// Called when any player lost connection in Host.
    /// </summary>
    public abstract void ConnectionLostHostAndClient();

    public abstract override void OnNetworkDespawn();
}
