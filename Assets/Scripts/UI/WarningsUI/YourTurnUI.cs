using System;
using UnityEngine;

public class YourTurnUI : BaseWarning
{
    public static event Action OnYourTurnSound;

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

    protected override void PlayWarningSound()
    {
        OnYourTurnSound?.Invoke();
    }
}
