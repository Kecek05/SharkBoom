using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class PlayerTutorialUi : MonoBehaviour
{
    [SerializeField] private PlayerTutorialController playerTutorialController;
    [SerializeField] private VideoPlayer tutorialVideoPlayer;
    [SerializeField] private TextMeshProUGUI tutorialTitle;
    [SerializeField] private GameObject tutorialPanel;


    private void Start()
    {
        playerTutorialController.OnTutorialSelected += HandleOnTutorialSelected;
    }

    private void HandleOnTutorialSelected(TutorialData tutorialData)
    {
        tutorialVideoPlayer.clip = tutorialData.tutorialVideo;
        tutorialTitle.text = tutorialData.tutorialTitle;
        Debug.Log($"TUTORIAL TEST - Recieve tutorial data, title: {tutorialData.tutorialTitle}, video: {tutorialData.tutorialVideo}");
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

    private void OnDestroy()
    {
        playerTutorialController.OnTutorialSelected -= HandleOnTutorialSelected;
    }
}
