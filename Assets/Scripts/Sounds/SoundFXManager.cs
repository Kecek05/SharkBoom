using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private AudioSource audioSource;

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

        // Item actions
        BaseItemThrowable.OnItemSpawnSound += PlayOnItemSpawned;
        BaseItemThrowable.OnItemReleasedAction += PlayOnItemLaunchedSound;
        BaseItemThrowableActivable.OnItemActivatedSound += PlayOnItemActivableSound;
        BaseCollisionController.OnCollidedSound += PlayOnItemOnCollidedSound;
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



    private void PlayOnItemSpawned(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemSpawn, itemTransform.position);
    }
    private void PlayOnItemLaunchedSound(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemLaunched, itemTransform.position);
    }

    private void PlayOnItemActivableSound(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemActivable, itemTransform.position);
    }
    private void PlayOnItemOnCollidedSound(Transform transform)
    {
        PlaySound(audioClipRefsSO.itemHit, transform.position);
    }



    public void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        if (audioClipArray == null || audioClipArray.Length == 0) return;

        if(position == null)
        {
            position = mainCamera.position;
        }

        audioSource.transform.position = position;
        audioSource.clip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        audioSource.volume = volume;
        audioSource.Play();
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

        // Item actions
        BaseItemThrowable.OnItemSpawnSound -= PlayOnItemSpawned;
        BaseItemThrowable.OnItemReleasedAction -= PlayOnItemLaunchedSound;
        BaseItemThrowableActivable.OnItemActivatedSound -= PlayOnItemActivableSound;
        BaseCollisionController.OnCollidedSound -= PlayOnItemOnCollidedSound;
    }
}


