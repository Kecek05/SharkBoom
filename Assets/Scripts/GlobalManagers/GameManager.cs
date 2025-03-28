using Sortify;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [BetterHeader("References")]
    [SerializeField] private ItemsListSO itemsListSO;
    [SerializeField] private List<Transform> spawnPointsPos;

    private BaseTurnManager turnManager;
    //private TimerManager timerManager;
    private BaseGameTimerManager gameTimerManager;
    //private PlayersPublicInfoManager playersPublicInfoManager;
    //private ItemActivableManager itemActivableManager;
    private BaseGameStateManager gameStateManager;
    private BaseGameOverManager gameOverManager;



    private void Awake()
    {
        instance = this;
        //CalculatePearlsManager.Reset();
    }

    private void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        //timerManager = ServiceLocator.Get<TimerManager>();
        gameTimerManager = ServiceLocator.Get<BaseGameTimerManager>();
        //playersPublicInfoManager = ServiceLocator.Get<PlayersPublicInfoManager>();
        //itemActivableManager = ServiceLocator.Get<ItemActivableManager>();
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();

        HandleEvents();
    }

    private void HandleEvents()
    {
        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChange;

        gameTimerManager.OnGameTimerEnd += HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned += HandleOnPlayerSpawned;

        PlayerHealth.OnPlayerDie += HandeOnPlayerDie;

        gameOverManager.LosedPlayer.OnValueChanged += HandleOnLosedPlayerValueChanged;

        turnManager.CurrentPlayableState.OnValueChanged += HandleOnPlayableStateChange;
    }

    private void HandleOnPlayableStateChange(PlayableState previousValue, PlayableState newValue)
    {
        turnManager.HandleOnPlayableStateValueChanged(previousValue, newValue);
    }

    private void HandeOnPlayerDie(PlayableState state)
    {
        gameOverManager.LoseGame(state);

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;
    }

    private void HandleOnLosedPlayerValueChanged(PlayableState previousValue, PlayableState newValue)
    {
        gameOverManager.HandleOnLosedPlayerValueChanged(previousValue, newValue);
    }

    private void HandleOnPlayerSpawned(int playerCount)
    {
        gameStateManager.HandleOnPlayerSpawned(playerCount);
    }

    private void HandleOnGameTimerEnd()
    {
        gameStateManager.HandleOnGameTimerEnd();
    }

    private void HandleOnGameStateChange(GameState previousValue, GameState newValue)
    {
        gameStateManager.HandleOnGameStateValueChanged(previousValue, newValue);
        gameTimerManager.HandleOnGameStateChange(newValue);
    }

    private void UnHandleEvents()
    {
        gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChange;

        gameTimerManager.OnGameTimerEnd -= HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned -= HandleOnPlayerSpawned;

        gameOverManager.LosedPlayer.OnValueChanged -= HandleOnLosedPlayerValueChanged;

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;

        gameOverManager.LosedPlayer.OnValueChanged -= HandleOnLosedPlayerValueChanged;

        turnManager.CurrentPlayableState.OnValueChanged -= HandleOnPlayableStateChange;
    }

    /// <summary>
    /// Call this to randomize and give items to players
    /// </summary>
    public void RandomizePlayerItems()
    {
        //int itemsInInventory = UnityEngine.Random.Range(2, itemsListSO.allItemsSOList.Count); //Random qtd of items for now
        int itemsInInventory = itemsListSO.allItemsSOList.Count; //all items

        //Add Jump item first
        foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
        {
            playerInventory.SetPlayerItems(0);
        }

        for (int i = 0; i < itemsInInventory; i++)
        {
            int randomItemSOIndex = UnityEngine.Random.Range(1, itemsListSO.allItemsSOList.Count); //Start from index 1,index 0 is jump

            foreach (PlayerInventory playerInventory in FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None))
            {
                playerInventory.SetPlayerItems(randomItemSOIndex);
            }
        }
    }

    /// <summary>
    /// Get a random Spawnpoint from the list and remove it.
    /// </summary>
    /// <returns></returns>
    public Transform GetRandomSpawnPoint()
    {
        Transform selectedSpawnPoint = spawnPointsPos[UnityEngine.Random.Range(0, spawnPointsPos.Count)];
        spawnPointsPos.Remove(selectedSpawnPoint);
        return selectedSpawnPoint;
    }


    private void OnDestroy()
    {
        UnHandleEvents();
    }

}




