using System.Collections;
using Unity.Netcode;

public class TimerManager : BaseTimerManager
{
    private BaseGameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
    }

    public override void HandleOnGameStateChanged(GameState gameState)
    {
        if(!IsServer) return;

        if(gameState == GameState.GameEnded)
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

    }

    public override void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(!IsServer) return;

        if (gameStateManager.CurrentGameState.Value == GameState.GameEnded) return;

        if (newValue == PlayableState.Player1Playing || newValue == PlayableState.Player2Playing)
        {
            timerTurn.Value = turnTime;
            isPaused = false; //unpause

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

    public override void TogglePauseTimer(bool isPaused)
    {
       PauseTimerServerRpc(isPaused);
    }

    [Rpc(SendTo.Server)]
    private void PauseTimerServerRpc(bool pause)
    {
        if (pause)
        {
            isPaused = true;
        }
        else
        {
            isPaused = false;
        }
    }
    

    protected override IEnumerator Timer()
    {
        while (timerTurn.Value > 0)
        {
            yield return timerDelay;

            if (isPaused) continue; //paused, skip

            timerTurn.Value--;
        }

        TriggerOnTurnTimesUp();
        TriggerOnTurnTimesUpClient();
        //time's up

        timerCoroutine = null;
    }

    protected override void TriggerOnTurnTimesUpClient()
    {
        TriggerOnTurnTimesUpClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnTurnTimesUpClientRpc()
    {
        TriggerOnTurnTimesUp();
    }
}
