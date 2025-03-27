using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameTimerManager : NetworkBehaviour
{
    
    public static event Action OnGameTimerEnd;

    /// <summary>
    /// Current time remaining in the Match in seconds
    /// </summary>
    private NetworkVariable<int> gameTimer = new(0);

    public NetworkVariable<int> GameTimer => gameTimer;
    /// <summary>
    /// Duration of the Match in seconds
    /// </summary>
    private const int startGameTimer = 300;

    private Coroutine gameTimerCoroutine;

    private WaitForSeconds timerDelay = new WaitForSeconds(1); //cache

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        GameStateManager.OnGameStateChange += GameStateManager_OnGameStateChange;
    }

    private void GameStateManager_OnGameStateChange(GameState gameState)
    {
        if(gameState == GameState.GameStarted)
        {
            if (gameTimerCoroutine == null)
            {
                gameTimerCoroutine = StartCoroutine(GameTimerTicks());
            }

        } else if (gameState == GameState.GameEnded)
        {
            if (gameTimerCoroutine != null)
            {
                StopCoroutine(gameTimerCoroutine);
                gameTimerCoroutine = null;
            }
        }
    }
    private IEnumerator GameTimerTicks()
    {
        gameTimer.Value = startGameTimer;

        while (gameTimer.Value > 0)
        {
            yield return timerDelay;
            gameTimer.Value--;
        }

        gameTimerCoroutine = null;
        OnGameTimerEnd?.Invoke();

    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        GameStateManager.OnGameStateChange -= GameStateManager_OnGameStateChange;
    }
}
