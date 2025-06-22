using System;
using UnityEngine;

public class TimersUpUI : BaseWarning
{
    public static event Action OnTurnTimersUpSound;
    private BaseTimerManager timerManager;

    protected override void Start()
    {
        timerManager = ServiceLocator.Get<BaseTimerManager>();

        timerManager.OnTurnTimesUp += StartWarning;
    }

    protected override void OnDestroy()
    {
        timerManager.OnTurnTimesUp -= StartWarning;
    }

    protected override void PlayWarningSound()
    {
        OnTurnTimersUpSound?.Invoke();
    }
}
