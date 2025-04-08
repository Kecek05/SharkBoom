using UnityEngine;

public class TimersUpUI : BaseWarning
{

    private BaseTurnManager turnManager;
    private BaseTimerManager timerManager;

    protected override void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        timerManager = ServiceLocator.Get<BaseTimerManager>();

        timerManager.OnTurnTimesUp += StartWarning;
    }

    protected override void StartWarning()
    {
        if(turnManager.LocalPlayableState == turnManager.CurrentPlayableState.Value)
        {
            base.StartWarning();
        }
    }

    protected override void OnDestroy()
    {
        timerManager.OnTurnTimesUp -= StartWarning;
    }


}
