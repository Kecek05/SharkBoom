using UnityEngine;

public class YourTurnUI : BaseWarning
{

    protected override void Start()
    {
        ServiceLocator.Get<BaseTurnManager>().OnMyTurnStarted += StartWarning;
    }

    protected override void OnDestroy()
    {
        ServiceLocator.Get<BaseTurnManager>().OnMyTurnStarted -= StartWarning;
    }
}
