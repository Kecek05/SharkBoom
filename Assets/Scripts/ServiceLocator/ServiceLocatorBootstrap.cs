using UnityEngine;

public class ServiceLocatorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        //Register clients
        BaseTurnManager turnManager = gameObject.AddComponent<BaseTurnManager>();
        BaseTimerManager timerManager = gameObject.AddComponent<BaseTimerManager>();
        BaseGameTimerManager gameTimerManager = gameObject.AddComponent<BaseGameTimerManager>();
        BasePlayersPublicInfoManager playersPublicInfoManager = gameObject.AddComponent<BasePlayersPublicInfoManager>();
        BaseItemActivableManager itemActivableManager = gameObject.AddComponent<BaseItemActivableManager>();
        BaseGameStateManager gameStateManager = gameObject.AddComponent<BaseGameStateManager>();
        BaseGameOverManager gameOverManager = gameObject.AddComponent<BaseGameOverManager>();

        ServiceLocator.Register(turnManager);
        ServiceLocator.Register(timerManager);
        ServiceLocator.Register(gameTimerManager);
        ServiceLocator.Register(playersPublicInfoManager);
        ServiceLocator.Register(itemActivableManager);
        ServiceLocator.Register(gameStateManager);
        ServiceLocator.Register(gameOverManager);
    }
}
