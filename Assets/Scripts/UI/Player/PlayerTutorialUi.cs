using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

public class PlayerTutorialUi : NetworkBehaviour
{
    [SerializeField] private PlayerTutorialController playerTutorialController;
    [SerializeField] private VideoPlayer tutorialVideoPlayer;
    [SerializeField] private TextMeshProUGUI tutorialTitle;
    [SerializeField] private GameObject tutorialPanel;

    public override void OnNetworkSpawn()
    {
        HideTutorialPanel();
    }
    public void InitializeOwner()
    {
        playerTutorialController.OnTutorialSelected += HandleOnTutorialSelected;
    }

    private void HandleOnTutorialSelected(TutorialData tutorialData)
    {
        tutorialVideoPlayer.clip = tutorialData.tutorialVideo;
        tutorialTitle.text = tutorialData.tutorialTitle;
    }

    public void ShowTutorialPanel()
    {
        tutorialPanel.SetActive(true);
        tutorialVideoPlayer.Play();
    }

    public void HideTutorialPanel()
    {
        tutorialPanel.SetActive(false);
        tutorialVideoPlayer.Stop();
    }

    public void UnInitializeOwner()
    {
        playerTutorialController.OnTutorialSelected -= HandleOnTutorialSelected;
    }
}
