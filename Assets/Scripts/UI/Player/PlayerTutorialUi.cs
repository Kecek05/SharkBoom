using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerTutorialUi : NetworkBehaviour
{
    [SerializeField] private PlayerTutorialController playerTutorialController;
    [SerializeField] private VideoPlayer tutorialVideoPlayer;
    [SerializeField] private TextMeshProUGUI tutorialTitle;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TutorialInfoSO defaultTutorial;
    [SerializeField] private Button tutorialButton;

    private TutorialData defaultTutorialData;

    public override void OnNetworkSpawn()
    {
        HideTutorialPanel();
    }
    public void InitializeOwner()
    {
        playerTutorialController.OnTutorialSelected += HandleOnTutorialSelected;
        tutorialVideoPlayer.prepareCompleted += HandleOnTutorialPrepared;

        defaultTutorialData = new TutorialData
        {
            tutorialVideo = defaultTutorial.tutorialVideo,
            tutorialTitle = defaultTutorial.tutorialTitle
        };

        HandleOnTutorialSelected(defaultTutorialData);
    }

    private void HandleOnTutorialPrepared(VideoPlayer source)
    {
        tutorialVideoPlayer.Play();
        tutorialPanel.SetActive(true);
        tutorialButton.interactable = true;
    }

    private void HandleOnTutorialSelected(TutorialData tutorialData)
    {
        tutorialVideoPlayer.clip = tutorialData.tutorialVideo;
        tutorialTitle.text = tutorialData.tutorialTitle;
    }

    public void ShowTutorialPanel()
    {
        tutorialVideoPlayer.Prepare();
        tutorialButton.interactable = false;
    }

    public void HideTutorialPanel()
    {
        tutorialPanel.SetActive(false);
        tutorialVideoPlayer.Stop();
    }

    public void UnInitializeOwner()
    {
        playerTutorialController.OnTutorialSelected -= HandleOnTutorialSelected;
        tutorialVideoPlayer.prepareCompleted -= HandleOnTutorialPrepared;
    }
}
