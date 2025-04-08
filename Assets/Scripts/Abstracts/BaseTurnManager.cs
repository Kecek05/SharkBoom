using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseTurnManager : NetworkBehaviour
{

    //Events

    public event Action OnMyTurnStarted; //local player can play
    public event Action OnMyTurnEnded;
    public event Action OnMyTurnJumped;
    public event Action OnLocalPlayableStateChanged;

    //Variables
    [BetterHeader("Settings")]
    [Tooltip("in ms")][SerializeField] protected int delayBetweenTurns = 3000;

    protected PlayableState localPlayableState = new();
    protected PlayableState localPlayedState = new();

    protected NetworkVariable<PlayableState> currentPlayableState = new(PlayableState.None);

    //Publics
    public PlayableState LocalPlayableState => localPlayableState;
    public NetworkVariable<PlayableState> CurrentPlayableState => currentPlayableState;

    //Methods

    protected void TriggerOnMyTurnStarted() => OnMyTurnStarted?.Invoke();

    protected void TriggerOnMyTurnEnded() => OnMyTurnEnded?.Invoke();

    protected void TriggerOnMyTurnJumped() => OnMyTurnJumped?.Invoke();

    protected void TriggerOnLocalPlayableStateChanged() => OnLocalPlayableStateChanged?.Invoke();


    /// <summary>
    /// Set the localPlayable and localPlayed states to the GameFlowManager, Only Client Owner.
    /// </summary>
    /// <param name="playingState"> Playing State</param>
    public abstract void InitializeLocalStates(PlayableState playingState);

    public abstract void StartGame();

    /// <summary>
    /// Call this when player played an item. It will change the turn.
    /// </summary>
    /// <param name="playerPlayingState"> Playing State</param>
    public abstract void PlayerPlayed(PlayableState playerPlayingState);

    /// <summary>
    /// Called when the player jumped.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    public abstract void PlayerJumped(PlayableState playableState);

    /// <summary>
    /// Tell the owner that the player jumped.
    /// </summary>
    /// <param name="playableState"> Owner Playing State</param>
    protected abstract void PlayerJumpedClient(PlayableState playableState);

    /// <summary>
    /// Call this to change the player's turn after a delay.
    /// </summary>
    /// <param name="playableState"> Playing State</param>
    protected abstract void DelayChangeTurns(PlayableState playableState);

    protected abstract void SetPlayableStateServer(PlayableState newState);

    public abstract void HandleOnGameStateChanged(GameState newValue);
    public abstract void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue);

    public abstract void HandleOnTimesUp();
}
