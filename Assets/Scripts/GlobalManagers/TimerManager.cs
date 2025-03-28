using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TimerManager : NetworkBehaviour
{
    public static event Action OnTimesUp;

    [Unit(" s")][SerializeField] private int turnTime = 30;
    private NetworkVariable<int> timerTurn = new(0);

    private Coroutine timerCoroutine;
    private WaitForSeconds timerDelay = new WaitForSeconds(1); //cache

    public NetworkVariable<int> TimerTurn => timerTurn;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GameManager.Instance.TurnManager.CurrentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;

        GameManager.Instance.GameStateManager.OnGameOver += GameFlowManager_OnGameOver;
    }



    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if (GameManager.Instance.GameStateManager.GameOver) return;

        if (newValue == PlayableState.Player1Playing || newValue == PlayableState.Player2Playing)
        {
            timerTurn.Value = turnTime;

            if (timerCoroutine == null)
            {
                timerCoroutine = StartCoroutine(Timer());
            }


        }
        else
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
    }

    private void GameFlowManager_OnGameOver()
    {

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        GameManager.Instance.TurnManager.CurrentPlayableState.OnValueChanged -= CurrentPlayableState_OnValueChanged;

        GameManager.Instance.GameStateManager.OnGameOver -= GameFlowManager_OnGameOver;
    }

    private IEnumerator Timer()
    {
        while (timerTurn.Value > 0)
        {
            yield return timerDelay;
            timerTurn.Value--;
        }

        OnTimesUp?.Invoke();

        //time's up
        GameManager.Instance.TurnManager.PlayerPlayedServerRpc(GameManager.Instance.TurnManager.CurrentPlayableState.Value); //change turn

        timerCoroutine = null;
    }

}
