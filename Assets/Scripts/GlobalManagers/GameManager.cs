using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    /// <summary>
    /// Called when the client gain ownership of the player object. Pass the new owner clientId
    /// </summary>
    public static event Action<ulong> OnClientOwnershipChanged;

    private BaseTurnManager turnManager;
    private BaseTimerManager timerManager;
    private BaseGameTimerManager gameTimerManager;
    //private PlayersPublicInfoManager playersPublicInfoManager;
    //private ItemActivableManager itemActivableManager;
    private BaseGameStateManager gameStateManager;
    private BaseGameOverManager gameOverManager;
    private BasePearlsManager pearlsManager;

    private int handledOwnershipEvents = 0;

    [SerializeField] private DamageableSO debugHitKillDamageableSO;

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
        Debug.Log("HandleEvents Game manager");

        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;

        gameTimerManager.OnGameTimerEnd += HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned += HandleOnPlayerSpawned;

        PlayerHealth.OnPlayerDie += HandeOnPlayerDie;

        turnManager.CurrentPlayableState.OnValueChanged += HandleOnPlayableStateChanged;

        gameOverManager.LosedPlayer.OnValueChanged += HandleOnLosedPlayerChanged;

        timerManager.OnTurnTimesUp += HandleOnTurnTimesUp;

        if(IsServer)
            OwnershipHandler.OnClientGainOwnership += HandleOnOwnershipHandlerClientGainOwnership;
    }

    private void HandleOnOwnershipHandlerClientGainOwnership(ulong clientId)
    {
        HandleOnOwnershipHandlerClientGainOwnershipToClientRpc(clientId);
        gameStateManager.HandleOnPlayerGainOwnership(clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandleOnOwnershipHandlerClientGainOwnershipToClientRpc(ulong clientId)
    {
        OnClientOwnershipChanged?.Invoke(clientId);
    }

    // Times up
    private void HandleOnTurnTimesUp()
    {
        turnManager.HandleOnTimesUp();
    }


    // Playable State
    private void HandleOnPlayableStateChanged(PlayableState previousValue, PlayableState newValue)
    {
        timerManager.HandleOnPlayableStateValueChanged(previousValue, newValue);
        turnManager.HandleOnPlayableStateValueChanged(previousValue, newValue);
    }

    // Player Die
    private void HandeOnPlayerDie()
    {
        gameStateManager.HandeOnPlayerDie();

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;
    }

    // Player Spawned
    private void HandleOnPlayerSpawned(int playerCount)
    {
        gameStateManager.HandleOnPlayerSpawned(playerCount);
    }

    // Game Timer End
    private void HandleOnGameTimerEnd()
    {
        gameStateManager.HandleOnGameTimerEnd();
    }

    // Game State
    private void HandleOnGameStateChanged(GameState previousValue, GameState newValue)
    {
        gameStateManager.HandleOnGameStateValueChanged(newValue);

        gameOverManager.HandleOnGameStateChanged(newValue); //Define the winner

        turnManager.HandleOnGameStateChanged(newValue);
        gameTimerManager.HandleOnGameStateChanged(newValue);
        timerManager.HandleOnGameStateChanged(newValue);
    }

    // Losed Player
    private void HandleOnLosedPlayerChanged(PlayableState previousValue, PlayableState newValue)
    {
        pearlsManager.HandleOnLosedPlayerChanged(newValue);

        gameOverManager.HandleOnLosedPlayerChanged(newValue);

    }

    private void UnHandleEvents()
    {
        Debug.Log("UnHandleEvents Game Manager");

        gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChanged;

        gameTimerManager.OnGameTimerEnd -= HandleOnGameTimerEnd;

        PlayerSpawner.OnPlayerSpawned -= HandleOnPlayerSpawned;

        PlayerHealth.OnPlayerDie -= HandeOnPlayerDie;

        turnManager.CurrentPlayableState.OnValueChanged -= HandleOnPlayableStateChanged;

        gameOverManager.LosedPlayer.OnValueChanged -= HandleOnLosedPlayerChanged;

        timerManager.OnTurnTimesUp -= HandleOnTurnTimesUp;

        if (IsServer)
            OwnershipHandler.OnClientGainOwnership -= HandleOnOwnershipHandlerClientGainOwnership;
    }

    public override void OnNetworkDespawn()
    {
        UnHandleEvents();
    }



    //DEBUG
    [Command("killPlayer")]
    private void SetGameOver(PlayableState playableState)
    {
        SetGameOverRpc(playableState);
    }

    [Rpc(SendTo.Server)]
    private void SetGameOverRpc(PlayableState playableState)
    {
        PlayerHealth playerHealth = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(playableState).GetComponent<PlayerHealth>();

        playerHealth.PlayerTakeDamage(debugHitKillDamageableSO, BodyPartEnum.Head);

    }

}




