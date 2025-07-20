using Sortify;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] menuTracks;
    [SerializeField] private AudioClip[] gameTracks;

    private AudioClip[] currentTracks;
    private Coroutine musicCoroutine;
    private static MusicManager instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Loader.OnCurrentSceneChanged += HandleOnCurrentSceneChanged;
    }

    private void Start()
    {
        currentTracks = null;
        HandleOnCurrentSceneChanged(Loader.CurrentScene);
    }

    private void HandleOnCurrentSceneChanged(Loader.Scene scene)
    {
        AudioClip[] chosenTracks;

        if (scene == Loader.Scene.MainMenu)
        {
            chosenTracks = menuTracks;
        }
        else if (scene == Loader.Scene.GameNetCodeTest)
        {
            chosenTracks = gameTracks;
        }
        else
        {
            chosenTracks = null;
        }

        if (chosenTracks == currentTracks)
            return;

        currentTracks = chosenTracks;

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        if (currentTracks == null || currentTracks.Length == 0)
            return;

        musicCoroutine = StartCoroutine(PlayMusicsOnScene(currentTracks));
    }

    private IEnumerator PlayMusicsOnScene(AudioClip[] musicTracks)
    {
        while (true)
        {
            if (musicTracks == null || musicTracks.Length == 0)
            {
                yield break;
            }

            int musicIndex = UnityEngine.Random.Range(0, musicTracks.Length);
            musicSource.clip = musicTracks[musicIndex];
            musicSource.Play();

            yield return new WaitForSeconds(musicSource.clip.length);
        }
    }

    private void OnDestroy()
    {
        Loader.OnCurrentSceneChanged -= HandleOnCurrentSceneChanged;
    }
}

