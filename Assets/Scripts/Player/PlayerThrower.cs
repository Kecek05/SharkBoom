using QFSW.QC;
using Sortify;
using Unity.Netcode;
using UnityEngine;

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
    [SerializeField] private Collider playerTouchColl;
    [SerializeField] private GameObject[] playerObjectsToChangeLayer;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    [SerializeField] private FollowSelectedSocketComponent followSelectedSocketComponent;
    [SerializeField] private PlayerRagdollEnabler playerRagdollEnabler;
    [SerializeField] private HitReceiveNetworkedComponent hitReceiveNetworked;
    [SerializeField] private PlayerKnockbackListener playerKnockbackListener;
    [SerializeField] private PlayerGetUp playerGetUp;
    [SerializeField] private PlayerJumpUI playerJumpUI;
    [SerializeField] private GameObject itemStuckSocket;
    [SerializeField] private Transform hipsTransform;
    [SerializeField] private ItemSO itemJumpSO;
    [SerializeField] private PlayerTutorialController playerTutorialController;
    [SerializeField] private PlayerTutorialUi playerTutorialUI;
    [SerializeField] private PlayerIndicatorUI playerIndicatorUI;

    private PlayerStateMachine playerStateMachine;

    private NetworkVariable<PlayableState> thisPlayableState = new();

    private BaseTurnManager turnManager;
    private BaseGameStateManager gameStateManager;

    //Publics

    public GameObject ItemStuckSocket => itemStuckSocket;
    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public NetworkVariable<PlayableState> ThisPlayableState => thisPlayableState;
    
    public Transform HipsTransform => hipsTransform;
    
    //DEBUG
    public PlayerInventory PlayerInventory => playerInventory;

    public override void OnNetworkSpawn()
    {
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 10000);
        
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        
        playerStateMachine = new PlayerStateMachine(this, playerDragController, playerInventory, false);
        
        HandleEvents();
        Initialize();
        //DEBUG
        
    }

    private void HandleOnClientOwnershipChanged(ulong newOwnerClientId)
    {
        if(newOwnerClientId == OwnerClientId)
        {
            HandleOwnerEvents();
            InitializeOwner();
        }
    }

    private void Initialize()
    {
        Debug.Log($"Events - Initialize - {gameObject.name}");
        
        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);
        
        playerTouchColl.enabled = false;
        
        playerRotateToAim.InitializeOwner();
        playerDragController.Initialize(itemJumpSO.rb);
        
        playerStateMachine.Initialize(PlayerState.IdleEnemyTurn);
        
        // Debug.Log($"SPAWNED PLAYER STATE MACHINE: {playerStateMachine} - CURRENT STATE: {playerStateMachine.CurrentState} - OBJ: {gameObject.name}");
        
        playerDetectFacingDirection.InitializeOwner();
        // playerInventory.Initialize();
        
        
    }

    private void InitializeOwner()
    {
        Debug.Log($"Events - InitializeOwner - {gameObject.name}");
        //Owner initialize code
        
        playerStateMachine.ChangeOwnership(IsOwner);
        
        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);
        
        playerDragController.InitializeOwner();
        
        cameraManager.InitializeOwner();
        
        playerInventory.InitializeOwner();
        playerLauncher.InitializeOwner();
        
        playerTouchColl.enabled = true;

        playerInventoryUI.InitializeOwner();
        playerTutorialUI.InitializeOwner();
    }

    private void HandleOwnerEvents()
    {
        Debug.Log($"Events - HandleOwnerEvents - {gameObject.name}");
        
        // turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;
        //
        // turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;
        //
        // turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;
        
        playerLauncher.OnItemLaunched += HandleOnItemLaunched;
        playerInventoryUI.OnItemSelectedByUI += HandleOnItemSelectedByUI;
        
        playerDragController.OnDragChange += HandleOnDragChange;
        playerDragController.OnDragCancelable += HandleOnDragCancelable;
        
        playerInventory.OnItemAdded += HandleOnItemAdded;
    }

    private void HandleEvents()
    {
        Debug.Log($"Events - HandleEvents - {gameObject.name}");
        GameManager.OnClientOwnershipChanged += HandleOnClientOwnershipChanged;

        thisPlayableState.OnValueChanged += PlayableStateInitialize;
        
        hitReceiveNetworked.OnHitReceive += HandleOnHitReceive;
        
        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;

        playerInventory.OnItemChanged += HandleOnItemChanged;
        playerInventory.OnItemSelected += HandleOnItemSelected;
        playerInventory.OnItemSelectedSO += HandleOnItemSelectedSO;
                
        playerDragController.OnDragStart += HandleOnDragStart;
        
        playerDetectFacingDirection.OnRotationChanged += HandleOnPlayerDetectFacingDirectionRotationChanged;
        
        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;
        
        playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected += OnPlayerSpawnItemOnHandItemSocketSelected;
        
        playerGetUp.OnPlayerGetUp += HandleOnPlayerGetUp;
        playerRagdollEnabler.OnRagdollDisabled += HandleOnRagdollDisabled;

        playerLauncher.OnLastItemSynced += HandleOnLastItemSynced;
        
        playerStateMachine.OnStateChanged += HandleOnStateChanged;
        
        BaseItemThrowable.OnItemCallbackAction += BaseItemThrowableOnOnItemCallbackAction;
        
        turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;
        
        turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;
        
        turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;

        turnManager.OnEnemyTurnStarted += GameFlowManager_OnEnemyTurnStarted;
    }

    private void UnHandleEvents()
    {
        GameManager.OnClientOwnershipChanged -= HandleOnClientOwnershipChanged;

        thisPlayableState.OnValueChanged -= PlayableStateInitialize;
        
        hitReceiveNetworked.OnHitReceive -= HandleOnHitReceive;
        
        gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChanged;

        playerInventory.OnItemChanged -= HandleOnItemChanged;
        playerInventory.OnItemSelected -= HandleOnItemSelected;
        playerInventory.OnItemSelectedSO -= HandleOnItemSelectedSO;

        playerStateMachine.OnStateChanged -= HandleOnStateChanged;
                
        playerDragController.OnDragStart -= HandleOnDragStart;
        
        playerDetectFacingDirection.OnRotationChanged -= HandleOnPlayerDetectFacingDirectionRotationChanged;
        
        playerAnimator.OnCrossfadeFinished -= HandleOnPlayerAnimatorCrossfadeFinished;
        
        playerSpawnItemOnHand.OnItemOnHandSpawned -= HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned -= HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected -= OnPlayerSpawnItemOnHandItemSocketSelected;
        
        playerGetUp.OnPlayerGetUp -= HandleOnPlayerGetUp;
        playerRagdollEnabler.OnRagdollDisabled -= HandleOnRagdollDisabled;

        playerLauncher.OnLastItemSynced -= HandleOnLastItemSynced;
        
        BaseItemThrowable.OnItemCallbackAction -= BaseItemThrowableOnOnItemCallbackAction;

        turnManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;
        
        turnManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;
        
        turnManager.OnMyTurnJumped -= GameFlowManager_OnMyTurnJumped;
    }

    private void UnHandleOwnerEvents()
    {
        playerDragController.OnDragChange -= HandleOnDragChange;
        playerDragController.OnDragCancelable -= HandleOnDragCancelable;
        playerLauncher.OnItemLaunched -= HandleOnItemLaunched;
        playerInventoryUI.OnItemSelectedByUI -= HandleOnItemSelectedByUI;
        
        playerInventory.OnItemAdded -= HandleOnItemAdded;
    }

    private void UnInitializeOwner()
    {
        playerTouchColl.enabled = false;
        playerStateMachine.ChangeOwnership(false);
        
        cameraManager.UnInitializeOwner();
        playerLauncher.UnInitializeOwner();
        playerInventoryUI.UnHandleInitializeOwner();
        playerTutorialUI.UnInitializeOwner();
    }

    private void UnInitialize()
    {

    }
    
    private void HandleOnLastItemSynced(Vector3 aimPosition)
    {
        //Used to the Owner tells the client that the drag has been released and its time to Lerp the aim position and spawn item
        playerRotateToAim.SyncAimPosition(aimPosition, () =>
        {
            //Finished Lerp Aim Position
            Debug.Log($"STEPS CLIENT 2 - AIM POSITION SYNCED - AIM POSITION: {aimPosition} - {gameObject.name} - Current SM Player: {playerStateMachine.CurrentPlayerState}");

            if (playerStateMachine.CurrentPlayerState != PlayerState.DraggingItem &&
                playerStateMachine.CurrentPlayerState != PlayerState.DraggingJump)
            {

                ChangePlayerState(PlayerState.DraggingItem);
                Debug.LogWarning($"STEPS CLIENT 2.1 - AIM POSITION SYNCED - SETTING PLAYER SM TO DRAGGING ITEM AIM POSITION: {aimPosition} - {gameObject.name} - Current SM Player: {playerStateMachine.CurrentPlayerState}");
            }
            playerDragController.InvokeOnDragRelease();
        });
    }
    
    // private void HandleOnDragRelease()
    // {
    //
    // }

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

    private void HandleOnItemSelectedByUI(int itemID)
    {
        playerInventory.SelectItemDataByItemInventoryID(itemID);
        // Debug.Log($"HandleOnItemSelectedByUI - Item ID: {itemID}");
    }

    private void HandleOnDragChange(float forcePercent, float angle)
    {
        playerDetectFacingDirection.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
        playerDragUi.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
        playerRotateToAim.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
    }

    private void HandleOnStateChanged(PlayerState state)
    {
        // Debug.Log($"PlayerThrower - Player State Changed to: {state} - OBJ: {gameObject.name}");
        cameraManager.HandleOnPlayerStateMachineStateChanged(state);
        playerInventory.HandleOnPlayerStateMachineStateChanged(state);

        playerLauncher.HandleOnPlayerStateMachineStateChanged(state);
        playerDragController.HandleOnPlayerStateMachineStateChanged(state);
        playerAnimator.HandleOnPlayerStateMachineStateChanged(state);

        playerRotateToAim.HandleOnPlayerStateMachineStateChanged(state);
        playerDetectFacingDirection.HandleOnPlayerStateMachineStateChanged(state);
        playerDragUi.HandleOnPlayerStateMachineStateChanged(state);

        playerInventoryUI.HandleOnPlayerStateMachineStateChanged(state);

        playerSpawnItemOnHand.HandleOnPlayerStateChanged(state);
        
    }

    private void HandleOnItemLaunched(int itemInventoryIndex)
    {
        playerInventory.HandleOnPlayerLauncherItemLaunched(itemInventoryIndex);
    }

    private void HandleOnItemSelected(int selectedItemInventoryID)
    {
        playerDragController.SetDragRb(playerInventory.GetSelectedItemSO().rb);

        playerInventoryUI.HandleOnPlayerInventoryItemSelected(selectedItemInventoryID);
        playerJumpUI.HandleOnPlayerInventoryItemSelected(selectedItemInventoryID);

        playerSpawnItemOnHand.HandleOnPlayerInventoryItemSelected(selectedItemInventoryID);
        
        playerTutorialController.HandleOnItemSelectedSO(selectedItemInventoryID);
    }

    private void HandleOnItemChanged(ItemInventoryData itemChanged)
    {
        playerInventoryUI.HandleOnPlayerInventoryItemChanged(itemChanged);
        playerJumpUI.HandleOnPlayerInventoryItemChanged(itemChanged);
    }

    private void HandleOnItemAdded(ItemInventoryData itemAdded)
    {
        playerInventoryUI.HandleOnPlayerInventoryItemAdded(itemAdded);
    }

    private void HandleOnGameStateChanged(GameState previousValue, GameState newValue)
    {
        if(newValue == GameState.GameEnded)
        {
            //playerStateMachine.TransitionTo(playerStateMachine.playerGameOverState);
            ChangePlayerState(PlayerState.PlayerGameOver);
        } else if (newValue == GameState.GameStarted)
        {
            playerDetectFacingDirection.SetupDetectFacingDirection();
            playerDetectFacingDirection.FaceOtherPlayer();
        }
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

    private void HandleOnHitReceive(bool isJump)
    {
        cameraManager.HandleOnPlayerHit(isJump);
    }
    private void OnPlayerSpawnItemOnHandItemSocketSelected(ItemSocket selectedSocket)
    {
        followSelectedSocketComponent.HandleOnPlayerSpawnItemOnHandOnItemSocketSelected(selectedSocket);
    }
    
    private void BaseItemThrowableOnOnItemCallbackAction(bool isOwnerOfTheItem)
    {
        playerGetUp.HandleOnItemCallbackAction(isOwnerOfTheItem);
    }

    private void HandleOnRagdollDisabled()
    {
        playerDetectFacingDirection.FaceOtherPlayer();
        playerRotateToAim.OnRagdollDisabled();
        cameraManager.HandleOnDisabledRagdoll();
    }

    private void HandleOnPlayerGetUp()
    {
        playerRagdollEnabler.HandleOnPlayerGetUp();
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
    }

    private void GameFlowManager_OnMyTurnJumped()
    {
        ChangePlayerState(PlayerState.IdleMyTurn);
        /*playerStateMachine.TransitionTo(playerStateMachine.idleMyTurnState);
        TransitionToIdleMyTurnStateServerRpc();*/
    }

    private void GameFlowManager_OnMyTurnEnded()
    {
        ChangePlayerState(PlayerState.MyTurnEnded);
        playerIndicatorUI.HidePlayerIndicator();
    }

    private void GameFlowManager_OnMyTurnStarted()
    {
        if (IsOwner)
        {
            //Im the owner of this object, the event recieved is right
            ChangePlayerState(PlayerState.MyTurnStarted);
            playerIndicatorUI.ShowPlayerIndicator();
        }
        else
        {
            ChangePlayerState(PlayerState.IdleEnemyTurn);
        }
    }
    
    private void GameFlowManager_OnEnemyTurnStarted()
    {
        if (IsOwner)
        {
            ChangePlayerState(PlayerState.IdleEnemyTurn);
        }
        else
        {
            ChangePlayerState(PlayerState.MyTurnStarted);
        }
    }
    
    //STATES
    
    /// <summary>
    /// Call this to change the player state.
    /// </summary>
    /// <param name="playerState"> The Next State</param>
    public void ChangePlayerState(PlayerState playerState)
    {
        if (playerStateMachine == null)
        {
            Debug.LogWarning("Player State Machine is null, cannot change state.");
            return;
        }
        
        // Debug.Log($"PlayerThrower - Changing Player State to: {playerState} - Old State Was: {playerStateMachine.CurrentState} - GameObject: {gameObject.name}");
        playerStateMachine.ChangeStateWithPlayerState(playerState);
        
        if(playerState != PlayerState.DragReleaseItem && playerState != PlayerState.DragReleaseJump)
        {
            //Dont sync DragRelease states, they are only for the owner
            //TransitionToStateServerRpc(playerState);
        }
    }
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void TransitionToStateServerRpc(PlayerState playerState)
    {
        TransitionToStateClientRpc(playerState);
    }

    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void TransitionToStateClientRpc(PlayerState playerState)
    {
        playerStateMachine.ChangeStateWithPlayerState(playerState);
    }
    
    //DEBUG
    [Command("player-passTurn", MonoTargetType.All)]
    private void PassTurn()
    {
        if(!IsOwner)
        {
            return;
        }
        
        ChangePlayerState(PlayerState.IdleEnemyTurn);
        //playerStateMachine.TransitionTo(playerStateMachine.idleEnemyTurnState);
        turnManager.PlayerPlayed(turnManager.LocalPlayableState);

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
            foreach (GameObject playerCollider in playerObjectsToChangeLayer)
            {
                playerCollider.layer = PlayersPublicInfoManager.PLAYER_1_LAYER;
            }
        }
        else
        {
            foreach (GameObject playerCollider in playerObjectsToChangeLayer)
            {
                playerCollider.layer = PlayersPublicInfoManager.PLAYER_2_LAYER;
            }
        }

        ServiceLocator.Get<BasePlayersPublicInfoManager>().AddPlayerToPlayersDictionary(newValue, gameObject);

    }

    public override void OnNetworkDespawn()
    {
        // Debug.Log($"Events - OnNetworkDespawn - {gameObject.name}");
        UnInitialize();
        UnHandleEvents();
    }

    public override void OnLostOwnership()
    {
        Debug.Log($"Events - OnLostOwnership - {gameObject.name} - Owner: {IsOwner}");
        UnInitializeOwner();
        UnHandleOwnerEvents();
    }
    
}
