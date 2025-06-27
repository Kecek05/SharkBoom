using System;
using UnityEngine;
using UnityEngine.Video;

public class GameTutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += HandleOnVideoPlayerPrepared;
    }

    private void HandleOnVideoPlayerPrepared(VideoPlayer source)
    {
        Debug.Log("Teste");
        loadingPanel.SetActive(false);
        source.Play();
    }

    public void FinishedTutorial()
    {
        Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
    }
}
