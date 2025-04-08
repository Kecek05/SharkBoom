using Sortify;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingPanelUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button cancelMatchmakingBtn;
    [SerializeField] private GameObject matchmakingPanel;


    private void Awake()
    {
        Hide();

        cancelMatchmakingBtn.onClick.AddListener(() =>
        {
            //Cancel Matchmaking
            Hide();
        });
    }


    private void Hide()
    {
        matchmakingPanel.SetActive(false);
    }

    private void Show()
    {
        matchmakingPanel.SetActive(true);
    }
}
