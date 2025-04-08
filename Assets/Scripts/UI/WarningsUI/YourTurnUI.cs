using UnityEngine;

public class YourTurnUI : BaseWarning
{
    private BaseTurnManager turnManager;

    protected override void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        turnManager.OnMyTurnStarted += StartWarning;
    }

    protected override void OnDestroy()
    {
        turnManager.OnMyTurnStarted -= StartWarning;
    }
}
