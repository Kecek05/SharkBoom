using UnityEngine;

public class EnemyTurnUI : BaseWarning
{
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
}
