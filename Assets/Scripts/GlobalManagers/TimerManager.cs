using System.Collections;

public class TimerManager : BaseTimerManager
{
    private BaseGameOverManager gameOverManager;
    private BaseTurnManager turnManager;

    private void Start()
    {
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();

        turnManager = ServiceLocator.Get<BaseTurnManager>();
    }

    public override void HandleOnGameOver()
    {
        if(!IsServer) return;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public override void HandleOnPlayableStateValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(!IsServer) return;

        if (gameOverManager.GameOver) return;

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

    protected override IEnumerator Timer()
    {
        while (timerTurn.Value > 0)
        {
            yield return timerDelay;
            timerTurn.Value--;
        }

        TriggerOnTurnTimesUp();

        //time's up
        turnManager.PlayerPlayedServerRpc(turnManager.CurrentPlayableState.Value); //change turn

        timerCoroutine = null;
    }
}
