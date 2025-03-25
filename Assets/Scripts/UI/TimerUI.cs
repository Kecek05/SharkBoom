using System;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TimerManager timerManager;

    private void Start()
    {
        timerManager.TimerTurn.OnValueChanged += TimerTurn_OnValueChanged;
    }

    private void TimerTurn_OnValueChanged(int previousValue, int newValue)
    {
        timerText.text = newValue.ToString();
    }
}
