using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseTimerManager : NetworkBehaviour
{
    public event Action OnTurnTimesUp;

    [Unit(" s")][SerializeField] protected int turnTime = 30;
    protected NetworkVariable<int> timerTurn = new(0);

    protected Coroutine timerCoroutine;
    protected WaitForSeconds timerDelay = new WaitForSeconds(1); //cache

    public NetworkVariable<int> TimerTurn => timerTurn;


    protected void TriggerOnTurnTimesUp() => OnTurnTimesUp?.Invoke();

    public abstract void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue);

    public abstract void HandleOnGameStateChanged(GameState gameState);

    protected abstract IEnumerator Timer();

}
