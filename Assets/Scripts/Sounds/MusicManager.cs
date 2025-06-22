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
        HandleOnCurrentSceneChanged(Loader.CurrentScene);
    }

    private void HandleOnCurrentSceneChanged(Loader.Scene scene)
    {
        AudioClip[] chosenTracks;

        if (scene == Loader.Scene.MainMenu || scene == Loader.Scene.NameBootstrap || scene == Loader.Scene.GameTutorial)
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

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }
        musicCoroutine = StartCoroutine(PlayMusicsOnScene(chosenTracks));

        if (chosenTracks == null) return;

        
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

