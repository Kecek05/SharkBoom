using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseGameTimerManager : NetworkBehaviour
{
    //Events
    public event Action OnGameTimerEnd;

    //Variables

    /// <summary>
    /// Current time remaining in the Match in seconds
    /// </summary>
    protected NetworkVariable<int> gameTimer = new(0);

    public NetworkVariable<int> GameTimer => gameTimer;
    /// <summary>
    /// Duration of the Match in seconds
    /// </summary>
    protected const int startGameTimer = 300;

    protected Coroutine gameTimerCoroutine;

    protected WaitForSeconds timerDelay = new WaitForSeconds(1); //cache

    //Methods

    protected void TriggerOnGameTimerEnd() => OnGameTimerEnd?.Invoke();

    public abstract void HandleOnGameStateChanged(GameState gameState);
    protected abstract IEnumerator GameTimerTicks();

}
