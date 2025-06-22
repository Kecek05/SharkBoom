using QFSW.QC;
using System;
using System.Collections;
using Unity.Netcode;

public class GameTimerManager : BaseGameTimerManager
{
    public static event Action OnGameTimerStartSound;
    public static event Action OnGameTimerEndSound;

    public override void HandleOnGameStateChanged(GameState gameState)
    {
        if (!IsServer) return;

        if (gameState == GameState.GameStarted)
        {
            if (gameTimerCoroutine == null)
            {
                gameTimerCoroutine = StartCoroutine(GameTimerTicks());
                OnGameTimerStartSound?.Invoke();
            }

        }
        else if (gameState == GameState.GameEnded)
        {
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
                OnGameTimerEndSound?.Invoke();
            }
        }
    }

    protected override IEnumerator GameTimerTicks()
    {
        gameTimer.Value = startGameTimer;

        while (gameTimer.Value > 0)
        {
            yield return timerDelay;
            gameTimer.Value--;
        }

        gameTimerCoroutine = null;
        TriggerOnGameTimerEnd();
    }


    //Debug
    [Command("StopGameTimer")]
    private void StopGameTimer()
    {
        StopGameTimerServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void StopGameTimerServerRpc()
    {
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
            TriggerOnGameTimerEnd();
        }
    }

}
