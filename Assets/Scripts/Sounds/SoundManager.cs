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

        // Item actions
        BaseItemThrowable.OnItemSpawnSound += PlayOnItemSpawned;
        BaseItemThrowable.OnItemReleasedAction += PlayOnItemLaunchedSound;
        BaseItemThrowableActivable.OnItemActivatedSound += PlayOnItemActivableSound;
        BaseCollisionController.OnCollidedSound += PlayOnItemOnCollidedSound;
    }

   

    private void PlayYourTurnSound()
    {
        PlaySound(audioClipRefsSO.startYourTurn, mainCamera.position);
        Debug.Log("SOUND - PlayYourTurnSound");
    }

    private void PlayEnemyYourTurnSound()
    {
        PlaySound(audioClipRefsSO.startEnemyTurn, mainCamera.position);
        Debug.Log("SOUND - PlayEnemyYourTurnSound");
    }

    private void PlayTurnTimersUpSound()
    {
        PlaySound(audioClipRefsSO.finishTurn, mainCamera.position);
        Debug.Log("SOUND - PlayTurnTimersUpSound");
    }



    private void PlayOnGameTimerStartSound()
    {
        PlaySound(audioClipRefsSO.gameStart, mainCamera.position);
        Debug.Log("SOUND - PlayOnGameTimerStartSound");
    }

    private void PlayOnGameTimerEndSound()
    {
        PlaySound(audioClipRefsSO.gameEnd, mainCamera.position);
        Debug.Log("SOUND - PlayOnGameTimerEndSound");
    }



    private void PlayOnGameWinSound()
    {
        PlaySound(audioClipRefsSO.gameWin, mainCamera.position);
        Debug.Log("SOUND - PlayOnGameWinSound");
    }

    private void PlayOnGameLoseSound()
    {
        PlaySound(audioClipRefsSO.gameLose, mainCamera.position);
        Debug.Log("SOUND - PlayOnGameLoseSound");
    }

    private void PlayOnGameTieSound()
    {
        PlaySound(audioClipRefsSO.gameTie, mainCamera.position);
        Debug.Log("SOUND - PlayOnGameTieSound");
    }



    private void PlayOnItemSpawned(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemSpawn, itemTransform.position);
        Debug.Log("SOUND - PlayOnDragStartSound");
    }
    private void PlayOnItemLaunchedSound(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemLaunched, itemTransform.position);
        Debug.Log("SOUND - PlayOnDragReleaseSound");
    }

    private void PlayOnItemActivableSound(Transform itemTransform)
    {
        PlaySound(audioClipRefsSO.itemActivable, itemTransform.position);
        Debug.Log("SOUND - PlayOnItemActivableSound");
    }
    private void PlayOnItemOnCollidedSound(Transform transform)
    {
        PlaySound(audioClipRefsSO.itemHit, transform.position);
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

        // Item actions
        BaseItemThrowable.OnItemSpawnSound -= PlayOnItemSpawned;
        BaseItemThrowable.OnItemReleasedAction -= PlayOnItemLaunchedSound;
        BaseItemThrowableActivable.OnItemActivatedSound -= PlayOnItemActivableSound;
        BaseCollisionController.OnCollidedSound -= PlayOnItemOnCollidedSound;
    }
}


