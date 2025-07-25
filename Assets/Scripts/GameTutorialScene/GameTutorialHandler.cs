using System;
using UnityEngine;
using UnityEngine.Video;

public class GameTutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        loadingPanel.SetActive(true);
        videoPlayer.prepareCompleted += HandleOnVideoPlayerPrepared;
        videoPlayer.Prepare();
    }

    private void HandleOnVideoPlayerPrepared(VideoPlayer source)
    {
        loadingPanel.SetActive(false);
        source.Play();
    }

    public void FinishedTutorial()
    {
        Loader.LoadNoLoadingScreen(Loader.Scene.MainMenu);
    }
}
