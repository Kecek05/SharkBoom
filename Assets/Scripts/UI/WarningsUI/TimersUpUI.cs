using UnityEngine;

public class TimersUpUI : BaseWarning
{
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


}
