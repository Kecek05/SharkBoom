using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameTimerManager gameTimerManager;

    private void Start()
    {
        gameTimerManager.GameTimer.OnValueChanged += GameTimer_OnValueChanged;
    }

    private void GameTimer_OnValueChanged(int previousValue, int newValue)
    {
        string formattedTime = $"{newValue / 60}:{newValue % 60:D2}";

        Debug.Log(formattedTime);
        timerText.text = formattedTime;
    }
}
