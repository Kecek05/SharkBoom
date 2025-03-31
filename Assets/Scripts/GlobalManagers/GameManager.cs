using Sortify;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private BaseTurnManager turnManager;
    private BaseTimerManager timerManager;
    private BaseGameTimerManager gameTimerManager;
    //private PlayersPublicInfoManager playersPublicInfoManager;
    //private ItemActivableManager itemActivableManager;
    private BaseGameStateManager gameStateManager;
    private BaseGameOverManager gameOverManager;
    private BasePearlsManager pearlsManager;


    public override void OnNetworkSpawn()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        timerManager = ServiceLocator.Get<BaseTimerManager>();
        gameTimerManager = ServiceLocator.Get<BaseGameTimerManager>();
        //playersPublicInfoManager = ServiceLocator.Get<PlayersPublicInfoManager>();
        //itemActivableManager = ServiceLocator.Get<ItemActivableManager>();
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();
        pearlsManager = ServiceLocator.Get<BasePearlsManager>();

        HandleEvents();
    }

    private void HandleEvents()
    {
        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChange;

        gameTimerManager.OnGameTimerEnd += HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned += HandleOnPlayerSpawned;

        PlayerHealth.OnPlayerDie += HandeOnPlayerDie;

        gameOverManager.OnGameOver += HandleOnGameOver;

        turnManager.CurrentPlayableState.OnValueChanged += HandleOnPlayableStateChange;
    }

    private void HandleOnGameOver()
    {
        gameStateManager.HandleOnGameOver();
        timerManager.HandleOnGameOver();
    }

    private void HandleOnPlayableStateChange(PlayableState previousValue, PlayableState newValue)
    {
        timerManager.HandleOnPlayableStateValueChanged(previousValue, newValue);
        turnManager.HandleOnPlayableStateValueChanged(previousValue, newValue);
    }

    private void HandeOnPlayerDie()
    {
        gameStateManager.HandeOnPlayerDie();

        gameOverManager.LoseGame();

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;
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
        gameStateManager.HandleOnGameStateValueChanged(newValue);

        pearlsManager.HandleOnGameStateChanged(newValue);
        turnManager.HandleOnGameStateChanged(newValue);
        gameTimerManager.HandleOnGameStateChange(newValue);
    }

    private void UnHandleEvents()
    {
        gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChange;

        gameTimerManager.OnGameTimerEnd -= HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned -= HandleOnPlayerSpawned;

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;


        gameOverManager.OnGameOver -= HandleOnGameOver;

        turnManager.CurrentPlayableState.OnValueChanged -= HandleOnPlayableStateChange;

    }

    public override void OnNetworkDespawn()
    {
        UnHandleEvents();
    }

}




