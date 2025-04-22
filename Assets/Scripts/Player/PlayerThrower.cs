using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerThrower : NetworkBehaviour
{


    [BetterHeader("References")]
    [SerializeField] private GameObject playerGFX;
    [SerializeField] private PlayerDragUi playerDragUi;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private PlayerFlipGfx playerFlipGfx;
    [SerializeField] private PlayerRotateToAim playerRotateToAim;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerLauncher playerLauncher;
    [SerializeField] private PlayerDetectFacingDirection playerDetectFacingDirection;
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;
    [SerializeField] private Collider2D playerTouchColl;
    [SerializeField] private GameObject[] playerColliders;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    private PlayerStateMachine playerStateMachine;

    private NetworkVariable<PlayableState> thisPlayableState = new();

    private BaseTurnManager turnManager;
    private BaseGameStateManager gameStateManager;

    //Publics
    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public NetworkVariable<PlayableState> ThisPlayableState => thisPlayableState;

    public override void OnNetworkSpawn()
    {
        GameManager.OnClientOwnershipChanged += HandleOnClientOwnershipChanged;

        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        thisPlayableState.OnValueChanged += PlayableStateInitialize;


        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        //if (!IsHost) // host will add itself twice
        //    PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        playerTouchColl.enabled = false;

        //DEBUG
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 10000);
        Debug.Log($"OnNetworkSpawn of PlayerThrower - Im owner? {IsOwner} - OwnerId: {OwnerClientId}");
    }

    private void HandleOnClientOwnershipChanged(ulong newOwnerClientId)
    {
        if (!IsOwner) return;

        Debug.Log($"HandleOnClientOwnershipChanged - new owner by event: {newOwnerClientId} - new owner obj: {OwnerClientId}");

        if(newOwnerClientId == OwnerClientId)
        {
            Debug.Log($"HandleOnClientOwnershipChanged, PlayerThrower Gained Ownership, new owner is: {newOwnerClientId} - Im {NetworkManager.LocalClientId}");

            InitializeOwner();
            HandleEvents();
            playerTouchColl.enabled = true;

            playerInventory.HandleOnGainOwnership();
            playerInventoryUI.HandleOnGainOwnership();

            Debug.Log("HandleOnClientOwnershipChanged, Ownership changed");
        }
    }

    private void InitializeOwner()
    {
        //Owner initialize code
        Debug.Log("InitializeOwner called");

        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

        turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;

        turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;

        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;

        playerStateMachine = new PlayerStateMachine(this, playerDragController, playerInventory);

        playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);

        cameraManager.InitializeOwner();
        playerDetectFacingDirection.InitializeOwner();
        playerRotateToAim.InitializeOwner();
        playerInventory.InitializeOwner();
        playerLauncher.InitializeOwner();
        playerDragController.InitializeOwner(playerInventory.GetItemSOByItemSOIndex(0).rb);
        playerRagdollEnabler.InitializeOwner();
    }

    private void HandleEvents()
    {
        Debug.Log($"HandleEvents PlayerThrower - Im owner? {IsOwner} - OwnerId: {OwnerClientId}  - My Client Id: {NetworkManager.LocalClientId}");

        playerInventory.OnItemAdded += HandleOnItemAdded;
        playerInventory.OnItemChanged += HandleOnItemChanged;
        playerInventory.OnItemSelected += HandleOnItemSelected;
        playerInventory.OnItemSelectedSO += HandleOnItemSelectedSO;

        playerLauncher.OnItemLaunched += HandleOnItemLaunched;

        if(playerStateMachine != null)
            playerStateMachine.OnStateChanged += HandleOnStateChanged;

        playerDragController.OnDragStart += HandleOnDragStart;
        playerDragController.OnDragChange += HandleOnDragChange;
        playerDragController.OnDragCancelable += HandleOnDragCancelable;

        playerInventoryUI.OnItemSelectedByUI += HandleOnItemSelectedByUI;

        playerDetectFacingDirection.OnRotationChanged += HandleOnPlayerDetectFacingDirectionRotationChanged;
        PlayerHealth.OnPlayerTakeDamage += HandleOnPlayerHealthPlayerTakeDamage;

        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;

        playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;

    }

    private void UnHandleEvents()
    {
        Debug.Log($"UnHandleEvents PlayerThrower - Im owner? {IsOwner} - OwnerId: {OwnerClientId} - My Client Id: {NetworkManager.LocalClientId}");

        playerInventory.OnItemAdded -= HandleOnItemAdded;
        playerInventory.OnItemChanged -= HandleOnItemChanged;
        playerInventory.OnItemSelected -= HandleOnItemSelected;
        playerInventory.OnItemSelectedSO -= HandleOnItemSelectedSO;

        playerLauncher.OnItemLaunched -= HandleOnItemLaunched;

        if (playerStateMachine != null)
            playerStateMachine.OnStateChanged -= HandleOnStateChanged;

        playerDragController.OnDragStart -= HandleOnDragStart;
        playerDragController.OnDragChange -= HandleOnDragChange;
        playerDragController.OnDragCancelable -= HandleOnDragCancelable;

        playerInventoryUI.OnItemSelectedByUI -= HandleOnItemSelectedByUI;

        playerDetectFacingDirection.OnRotationChanged -= HandleOnPlayerDetectFacingDirectionRotationChanged;
        PlayerHealth.OnPlayerTakeDamage -= HandleOnPlayerHealthPlayerTakeDamage;

        playerAnimator.OnCrossfadeFinished -= HandleOnPlayerAnimatorCrossfadeFinished;

        playerSpawnItemOnHand.OnItemOnHandSpawned -= HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned -= HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;

        cameraManager.UnInitializeOwner();
        playerLauncher.UnInitializeOwner();
        playerInventoryUI.UnHandleInitializeOwner();
        playerRagdollEnabler.UnInitializeOwner();
    }

    private void HandleOnPlayerDetectFacingDirectionRotationChanged(bool isRight)
    {
        playerAnimator.HandleOnRotationChanged(isRight);
        playerFlipGfx.HandleOnRotationChanged(isRight);

        playerSpawnItemOnHand.HandleOnRotationChanged(isRight);
    }

    private void HandleOnDragStart()
    {
        playerDragUi.HandleOnPlayerDragControllerDragStart();
    }

    private void HandleOnDragCancelable(bool cancelable)
    {
        playerDragUi.HandleOnPlayerDragControllerDragCancelable(cancelable);
    }

    private void HandleOnItemSelectedSO(ItemSO itemSOSelected)
    {
        playerAnimator.HandleOnItemSelectedSO(itemSOSelected);
        playerInventoryUI.UpdateOpenInventoryButton(itemSOSelected.itemIcon);

    }

    private void HandleOnItemSelectedByUI(int itemInventoryIndex)
    {
        playerInventory.SelectItemDataByItemInventoryIndex(itemInventoryIndex);
    }

    private void HandleOnDragChange(float forcePercent, float angle)
    {
        playerDetectFacingDirection.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
        playerDragUi.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
        playerRotateToAim.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
    }

    private void HandleOnStateChanged(PlayerState state)
    {
        cameraManager.HandleOnPlayerStateMachineStateChanged(state);
        playerInventory.HandleOnPlayerStateMachineStateChanged(state);

        playerLauncher.HandleOnPlayerStateMachineStateChanged(state);
        playerDragController.HandleOnPlayerStateMachineStateChanged(state);
        playerAnimator.HandleOnPlayerStateMachineStateChanged(state);

        playerRotateToAim.HandleOnPlayerStateMachineStateChanged(state);
        playerDetectFacingDirection.HandleOnPlayerStateMachineStateChanged(state);
        playerDragUi.HandleOnPlayerStateMachineStateChanged(state);

        playerInventoryUI.HandleOnPlayerStateMachineStateChanged(state);
        playerRagdollEnabler.HandleOnPlayerStateChanged(state);


        playerSpawnItemOnHand.HandleOnPlayerStateChanged(state);
    }

    private void HandleOnItemLaunched(int itemInventoryIndex)
    {
        playerInventory.HandleOnPlayerLauncherItemLaunched(itemInventoryIndex);
    }

    private void HandleOnItemSelected(int selectedItemInventoryIndex)
    {
        playerDragController.SetDragRb(playerInventory.GetSelectedItemSO().rb);

        playerInventoryUI.HandleOnPlayerInventoryItemSelected(selectedItemInventoryIndex);

        playerSpawnItemOnHand.HandleOnPlayerInventoryItemSelected(selectedItemInventoryIndex);
    }

    private void HandleOnItemChanged(ItemInventoryData itemChanged)
    {
        playerInventoryUI.HandleOnPlayerInventoryItemChanged(itemChanged);
    }

    private void HandleOnItemAdded(ItemInventoryData itemAdded)
    {
        playerInventoryUI.HandleOnPlayerInventoryItemAdded(itemAdded);
    }

    private void HandleOnGameStateChanged(GameState previousValue, GameState newValue)
    {
        if(newValue == GameState.GameEnded)
        {
            playerStateMachine.TransitionTo(playerStateMachine.playerGameOverState);
        }
    }

    private void HandleOnPlayerHealthPlayerTakeDamage(object sender, PlayerHealth.OnPlayerTakeDamageArgs e)
    {
        playerRagdollEnabler.HandleOnPlayerTakeDamage(sender, e);
    }

    private void HandleOnPlayerAnimatorCrossfadeFinished()
    {
        playerSpawnItemOnHand.HandleOnCrossfadeFinished();
    }

    private void HandleOnPlayerSpawnItemOnHandItemOnHandSpawned(BaseItemThrowable throwable)
    {
        playerLauncher.HandleOnItemOnHandSpawned(throwable);
    }

    private void HandleOnPlayerSpawnItemOnHandItemOnHandDespawned(BaseItemThrowable throwable)
    {
       playerLauncher.HandleOnItemOnHandDespawned(throwable);
    }

    //---

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

    //DEBUG
    [Command("checkImOwner", MonoTargetType.All)]
    private void CheckImOwner()
    {
        Debug.Log($"Server is the Owner? {IsOwnedByServer} - OwnerId: {OwnerClientId}");

        if (!IsOwner)
        {
            return;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y + 5f, transform.position.z);
    }

    private void PlayableStateInitialize(PlayableState previousValue, PlayableState newValue)
    {
        if (IsOwner && IsClient)
        {
            if(IsHost)
            {
                //Host always is Player1Playing
                ServiceLocator.Get<BaseTurnManager>().InitializeLocalStates(PlayableState.Player1Playing);

            } else
            {
                ServiceLocator.Get<BaseTurnManager>().InitializeLocalStates(newValue); //pass to GameFlow to know when its local turn
            }

           
        }

        if (newValue == PlayableState.Player1Playing)
        {

            foreach (GameObject playerCollider in playerColliders)
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


        ServiceLocator.Get<BasePlayersPublicInfoManager>().AddPlayerToPlayersDictionary(newValue, gameObject);

    }

    public override void OnNetworkDespawn()
    {
        GameManager.OnClientOwnershipChanged -= HandleOnClientOwnershipChanged;
        thisPlayableState.OnValueChanged -= PlayableStateInitialize;

        if (IsOwner)
        {
            turnManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;

            turnManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;

            turnManager.OnMyTurnJumped -= GameFlowManager_OnMyTurnJumped;

            gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChanged;

            UnHandleEvents();
        }



    }

    public override void OnLostOwnership()
    {
        Debug.Log($"Lost Ownership - called before changing the owner");

        playerTouchColl.enabled = false;


        UnHandleEvents();
    }
}
