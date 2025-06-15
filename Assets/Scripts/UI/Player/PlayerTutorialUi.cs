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
    }

    public void ShowTutorialPanel()
    {
        tutorialPanel.SetActive(true);
    }

    public void HideTutorialPanel()
    {
        tutorialPanel.SetActive(false);
    }

}
