using QFSW.QC;
using Sortify;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerThrower : NetworkBehaviour
{
    public static event Action<PlayerThrower> OnPlayerSpawned;


    [BetterHeader("References")]
    [SerializeField] private GameObject playerGFX;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerLauncher playerLauncher;
    [SerializeField] private BoxCollider playerTouchColl;
    [SerializeField] private GameObject[] playerColliders;
    private PlayerStateMachine playerStateMachine;

    private NetworkVariable<PlayableState> thisPlayableState = new();

    private BaseTurnManager turnManager;
    private BaseGameOverManager gameOverManager;

    //Publics
    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerInventoryUI PlayerInventoryUI => playerInventoryUI;
    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerDragController PlayerDragController => playerDragController;
    public PlayerLauncher PlayerLauncher => playerLauncher;
    public NetworkVariable<PlayableState> ThisPlayableState => thisPlayableState;



    private void Start()
    {
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        gameOverManager = ServiceLocator.Get<BaseGameOverManager>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {

            PlayerData playerData = NetworkServerProvider.Instance.CurrentNetworkServer.ServerAuthenticationService.GetPlayerDataByClientId(OwnerClientId);

            OnPlayerSpawned?.Invoke(this);

            gameObject.name = "Player " + playerData.userData.userName; //Debug
        }



        thisPlayableState.OnValueChanged += PlayableStateInitialize;

        if(!IsHost) // host will add itself twice
            PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        if (IsOwner)
        {

            turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

            turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;

            turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;

            gameOverManager.OnGameOver += GameFlowManager_OnGameOver;

            playerStateMachine = new PlayerStateMachine(this);

            playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);

        } else
        {
            //if not owner, turn off touch collider
            playerTouchColl.enabled = false;

        }

    }



    [Rpc(SendTo.Server)]
    public void InitializePlayerRpc(PlayableState playableState, Quaternion GFXRotation)
    {
        // Cant be OnnetworkSpawn because it needs to be called by NetworkServer
        thisPlayableState.Value = playableState;

        InitializeGFXRotationRpc(GFXRotation);
    }

    [Rpc(SendTo.Owner)]
    private void InitializeGFXRotationRpc(Quaternion GFXRotation)
    {
        playerGFX.transform.rotation = GFXRotation;
        Debug.Log($"Rotating: {playerGFX.transform.parent.name} to {GFXRotation}");
    }

    private void GameFlowManager_OnMyTurnJumped()
    {
        playerStateMachine.TransitionTo(playerStateMachine.idleMyTurnState);
    }

    private void GameFlowManager_OnMyTurnEnded()
    {
        playerStateMachine.TransitionTo(playerStateMachine.myTurnEndedState);
    }

    private void GameFlowManager_OnMyTurnStarted()
    {
        // My Turn Started, I can play
        playerStateMachine.TransitionTo(playerStateMachine.myTurnStartedState);
        Debug.Log("I can play!");

    }

    private void GameFlowManager_OnGameOver()
    {
        playerStateMachine.TransitionTo(playerStateMachine.playerGameOverState);
    }

    //DEBUG
    [Command("player-passTurn", MonoTargetType.All)]
    private void PassTurn()
    {
        if(!IsOwner)
        {
            return;
        }

        playerStateMachine.TransitionTo(playerStateMachine.idleEnemyTurnState);
        turnManager.PlayerPlayed(turnManager.LocalPlayableState);

    }

    

    private void PlayableStateInitialize(PlayableState previousValue, PlayableState newValue)
    {
        if (IsOwner)
        {
            turnManager.InitializeLocalStates(thisPlayableState.Value); //pass to GameFlow to know when its local turn
        }

        if (thisPlayableState.Value == PlayableState.Player1Playing)
        {

            foreach(GameObject playerCollider in playerColliders)
            {
                playerCollider.layer = PlayersPublicInfoManager.PLAYER_1_LAYER;
            }
        }
        else
        {
            foreach (GameObject playerCollider in playerColliders)
            {
                playerCollider.layer = PlayersPublicInfoManager.PLAYER_2_LAYER;
            }
        }

        PlayersPublicInfoManager.Instance.AddPlayerToPlayersDictionary(thisPlayableState.Value, gameObject);
    }

    public override void OnNetworkDespawn()
    {
        thisPlayableState.OnValueChanged -= PlayableStateInitialize;

        if (IsOwner)
        {
            turnManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;

            turnManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;

            turnManager.OnMyTurnJumped -= GameFlowManager_OnMyTurnJumped;

            gameOverManager.OnGameOver -= GameFlowManager_OnGameOver;
        }
    }

}
