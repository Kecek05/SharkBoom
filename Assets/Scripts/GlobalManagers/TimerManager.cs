using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TimerManager : NetworkBehaviour
{
    public event Action OnTimesUp;

    [Unit(" s")][SerializeField] private int turnTime = 30;
    private NetworkVariable<int> timerTurn = new(0);

    private Coroutine timerCoroutine;
    private WaitForSeconds timerDelay = new WaitForSeconds(1);


    public NetworkVariable<int> TimerTurn => timerTurn;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GameFlowManager.Instance.CurrentPlayableState.OnValueChanged += CurrentPlayableState_OnValueChanged;
    }

    private void CurrentPlayableState_OnValueChanged(PlayableState previousValue, PlayableState newValue)
    {
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

    private IEnumerator Timer()
    {
        while (timerTurn.Value > 0)
        {
            yield return timerDelay;
            timerTurn.Value--;
        }

        OnTimesUp?.Invoke();

        //time's up
        GameFlowManager.Instance.PlayerPlayedRpc(GameFlowManager.Instance.CurrentPlayableState.Value); //change turn

        timerCoroutine = null;
    }

}
