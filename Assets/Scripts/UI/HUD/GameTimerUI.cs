using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        ServiceLocator.Get<BaseGameTimerManager>().GameTimer.OnValueChanged += GameTimer_OnValueChanged;
    }

    private void GameTimer_OnValueChanged(int previousValue, int newValue)
    {
        string formattedTime = $"{newValue / 60}:{newValue % 60:D2}";

        timerText.text = formattedTime;
    }
}
