using UnityEngine;

public class ServiceLocatorBootstrap : MonoBehaviour
{
    private void Awake()
    {
        //Register clients
        ServiceLocator.Register(new TurnManager());
        ServiceLocator.Register(new TimerManager());
        ServiceLocator.Register(new GameTimerManager());
        ServiceLocator.Register(new PlayersPublicInfoManager());
        ServiceLocator.Register(new ItemActivableManager());
        ServiceLocator.Register(new GameStateManager());
        ServiceLocator.Register(new GameOverManager());
    }
}
