using Sortify;
using System;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ServiceLocatorBootstrap : MonoBehaviour
{
    public static event Action OnServiceLocatorInitialized;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO; //REFACTOR
    [SerializeField] private Transform[] spawnPointsPos; //REFACTOR

    private void Awake()
    {
        //Register clients
        BaseTurnManager turnManager = gameObject.AddComponent<TurnManager>();
        BaseTimerManager timerManager = gameObject.AddComponent<TimerManager>();
        BaseGameTimerManager gameTimerManager = gameObject.AddComponent<GameTimerManager>();

        BasePlayersPublicInfoManager playersPublicInfoManager = gameObject.AddComponent<PlayersPublicInfoManager>();
        playersPublicInfoManager.Initialize(itemsListSO);
        foreach (Transform spawnPoint in spawnPointsPos)
        {
            playersPublicInfoManager.AddRandomSpawnPoint(spawnPoint);
        }

        BaseItemActivableManager itemActivableManager = gameObject.AddComponent<ItemActivableManager>();
        BaseGameStateManager gameStateManager = gameObject.AddComponent<GameStateManager>();
        BaseGameOverManager gameOverManager = gameObject.AddComponent<GameOverManager>();
        BasePearlsManager pearlsManager = gameObject.AddComponent<PearlsManager>();

        ServiceLocator.Register(turnManager);
        ServiceLocator.Register(timerManager);
        ServiceLocator.Register(gameTimerManager);
        ServiceLocator.Register(playersPublicInfoManager);
        ServiceLocator.Register(itemActivableManager);
        ServiceLocator.Register(gameStateManager);
        ServiceLocator.Register(gameOverManager);
        ServiceLocator.Register(pearlsManager);



        OnServiceLocatorInitialized?.Invoke();
    }

}
