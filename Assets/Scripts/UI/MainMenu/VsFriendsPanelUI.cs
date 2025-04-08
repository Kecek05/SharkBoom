using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VsFriendsPanelUI : MonoBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Button closeVsFriendsPanelBtn;
    [SerializeField] private Button joinGameBtn;
    [SerializeField] private Button createGameBtn;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private GameObject vsFriendsPanel;


    private void Awake()
    {
        Hide();


    }


    private void Hide()
    {
        vsFriendsPanel.SetActive(false);
    }

    private void Show()
    {
        vsFriendsPanel.SetActive(true);
    }
}
