using System;
using System.Collections;
using UnityEngine;

public class GameTimerManager : MonoBehaviour
{
    
    public static event Action OnGameTimerEnd;

    /// <summary>
    /// Current time remaining in the Match in seconds
    /// </summary>
    private int gameTimer;

    /// <summary>
    /// Duration of the Match in seconds
    /// </summary>
    private const int startGameTimer = 15;


    private WaitForSeconds timerDelay = new WaitForSeconds(1); //cache

    private void Start()
    {
        gameTimer = startGameTimer;
        StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while (gameTimer > 0)
        {
            yield return timerDelay;
            gameTimer--;
        }

        OnGameTimerEnd?.Invoke();

    }
}
