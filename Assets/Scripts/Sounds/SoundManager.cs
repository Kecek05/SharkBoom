using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;
    [SerializeField] private Transform mainCamera;

    private void Start()
    {
        // Warnings events
        YourTurnUI.OnYourTurnSound += PlayYourTurnSound;
        EnemyTurnUI.OnEnemyTurnSound += PlayEnemyYourTurnSound;
        TimersUpUI.OnTurnTimersUpSound += PlayTurnTimersUpSound;

        // GameStates events
        GameTimerManager.OnGameTimerStartSound += PlayOnGameTimerStartSound;
        GameTimerManager.OnGameTimerEndSound += PlayOnGameTimerEndSound;

        // GameOver events
        GameOverManager.OnGameWinSound += PlayOnGameWinSound;
        GameOverManager.OnGameLoseSound += PlayOnGameLoseSound;
        GameOverManager.OnGameTieSound += PlayOnGameTieSound;

        // Players actions

    }
    

    private void PlayYourTurnSound()
    {
        PlaySound(audioClipRefsSO.startYourTurn, mainCamera.position);
    }

    private void PlayEnemyYourTurnSound()
    {
        PlaySound(audioClipRefsSO.startEnemyTurn, mainCamera.position);
    }

    private void PlayTurnTimersUpSound()
    {
        PlaySound(audioClipRefsSO.finishTurn, mainCamera.position);
    }



    private void PlayOnGameTimerStartSound()
    {
        PlaySound(audioClipRefsSO.gameStart, mainCamera.position);
    }

    private void PlayOnGameTimerEndSound()
    {
        PlaySound(audioClipRefsSO.gameEnd, mainCamera.position);
    }



    private void PlayOnGameWinSound()
    {
        PlaySound(audioClipRefsSO.gameWin, mainCamera.position);
    }

    private void PlayOnGameLoseSound()
    {
        PlaySound(audioClipRefsSO.gameLose, mainCamera.position);
    }

    private void PlayOnGameTieSound()
    {
        PlaySound(audioClipRefsSO.gameTie, mainCamera.position);
    }



    public static void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    { 
        AudioSource.PlayClipAtPoint(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void OnDestroy()
    {
        // Warnings events
        YourTurnUI.OnYourTurnSound -= PlayYourTurnSound;
        EnemyTurnUI.OnEnemyTurnSound -= PlayEnemyYourTurnSound;
        TimersUpUI.OnTurnTimersUpSound -= PlayTurnTimersUpSound;

        // GameStates events
        GameTimerManager.OnGameTimerStartSound -= PlayOnGameTimerStartSound;
        GameTimerManager.OnGameTimerEndSound -= PlayOnGameTimerEndSound;

        // GameOver events
        GameOverManager.OnGameWinSound -= PlayOnGameWinSound;
        GameOverManager.OnGameLoseSound -= PlayOnGameLoseSound;
        GameOverManager.OnGameTieSound -= PlayOnGameTieSound;

        // Players actions
    }
}


