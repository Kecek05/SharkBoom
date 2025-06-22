using System;

public class EnemyTurnUI : BaseWarning
{
    public static event Action OnEnemyTurnSound;
    private BaseTurnManager turnManager;

    protected override void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        turnManager.OnEnemyTurnStarted += StartWarning;
    }

    protected override void OnDestroy()
    {
        turnManager.OnEnemyTurnStarted -= StartWarning;
    }

    protected override void PlayWarningSound()
    {
        OnEnemyTurnSound?.Invoke();
    }
}
