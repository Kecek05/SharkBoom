using Sortify;
using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ServiceLocatorBootstrap : MonoBehaviour
{
    public static event Action OnServiceLocatorInitialized;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private Transform[] spawnPointsPos;
    [SerializeField] private CameraObjectToFollow cameraObjectToFollow;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Camera cameraMain;

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

        cameraObjectToFollow.gameObject.transform.position = new Vector3(12, 0, -20);

        ServiceLocator.Register(turnManager);
        ServiceLocator.Register(timerManager);
        ServiceLocator.Register(gameTimerManager);
        ServiceLocator.Register(playersPublicInfoManager);
        ServiceLocator.Register(itemActivableManager);
        ServiceLocator.Register(gameStateManager);
        ServiceLocator.Register(gameOverManager);
        ServiceLocator.Register(pearlsManager);
        ServiceLocator.Register(cameraObjectToFollow);
        ServiceLocator.Register(cinemachineCamera);
        ServiceLocator.Register(cameraMain);



        OnServiceLocatorInitialized?.Invoke();
    }

}
