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
        GameManager.OnClientOwnershipChanged += HandleOnClientOwnershipChanged;

        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        thisPlayableState.OnValueChanged += PlayableStateInitialize;

        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        playerTouchColl.enabled = false;

        //DEBUG
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 10000);
        
        hitReceiveNetworked.OnHitReceive += HandleOnHitReceive;
        
        playerRotateToAim.InitializeOwner();
        playerDragController.Initialize(itemJumpSO.rb);
        
        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;

        playerInventory.OnItemChanged += HandleOnItemChanged;
        playerInventory.OnItemSelected += HandleOnItemSelected;
        playerInventory.OnItemSelectedSO += HandleOnItemSelectedSO;
        
        playerLauncher.OnItemLaunched += HandleOnItemLaunched;
        
        playerStateMachine = new PlayerStateMachine(this, playerDragController, playerInventory);

        playerStateMachine.Initialize(PlayerState.IdleEnemyTurn);
        
        if(playerStateMachine != null)
            playerStateMachine.OnStateChanged += HandleOnStateChanged;
        
                
        playerDragController.OnDragStart += HandleOnDragStart;
        
        playerInventoryUI.OnItemSelectedByUI += HandleOnItemSelectedByUI;
        Debug.Log("PlayerThrower - Player Inventory UI Initialized");
        
        playerDetectFacingDirection.OnRotationChanged += HandleOnPlayerDetectFacingDirectionRotationChanged;
        
        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;
        
        playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected += OnPlayerSpawnItemOnHandItemSocketSelected;
        
        playerGetUp.OnPlayerGetUp += HandleOnPlayerGetUp;
        playerRagdollEnabler.OnRagdollDisabled += HandleOnRagdollDisabled;
        
        
        playerInventory.Initialize();

    }

    private void HandleOnClientOwnershipChanged(ulong newOwnerClientId)
    {
        if (!IsOwner) return;

        if(newOwnerClientId == OwnerClientId)
        {

            InitializeOwner();
            HandleEvents();
            playerTouchColl.enabled = true;

            playerInventory.HandleOnGainOwnership();
            playerInventoryUI.HandleOnGainOwnership();
        }
    }

    private void InitializeOwner()
    {
        //Owner initialize code

        PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);
        
        playerDragController.InitializeOwner();
        
        turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

        turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;

        turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;
        
        
        playerDragController.OnDragChange += HandleOnDragChange;
        playerDragController.OnDragCancelable += HandleOnDragCancelable;
        playerDragController.OnDragRelease += HandleOnDragRelease;
        /*
        turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

        turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;

        turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;*/

        /*gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;*/

        /*BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;*/

        /*hitReceiveNetworked.OnHitReceive += HandleOnHitReceive;*/

        cameraManager.InitializeOwner();
        playerDetectFacingDirection.InitializeOwner();
        //playerRotateToAim.InitializeOwner();
        playerInventory.InitializeOwner();
        playerLauncher.InitializeOwner();
        
        playerInventory.OnItemAdded += HandleOnItemAdded;
        
        BaseItemThrowable.OnItemCallbackAction += HandleOnItemCallbackAction;
        
        //playerDragController.InitializeOwner(playerInventory.GetItemSOByItemSOIndex(0).rb);
    }

    private void HandleEvents()
    {

        /*playerInventory.OnItemAdded += HandleOnItemAdded;
        playerInventory.OnItemChanged += HandleOnItemChanged;*/
        /*playerInventory.OnItemSelected += HandleOnItemSelected;
        playerInventory.OnItemSelectedSO += HandleOnItemSelectedSO;*/

        /*playerLauncher.OnItemLaunched += HandleOnItemLaunched;*/

        /*if(playerStateMachine != null)
            playerStateMachine.OnStateChanged += HandleOnStateChanged;*/

        /*
        playerDragController.OnDragStart += HandleOnDragStart;
        playerDragController.OnDragChange += HandleOnDragChange;
        playerDragController.OnDragCancelable += HandleOnDragCancelable;
        */

        /*playerInventoryUI.OnItemSelectedByUI += HandleOnItemSelectedByUI;*/

        /*playerDetectFacingDirection.OnRotationChanged += HandleOnPlayerDetectFacingDirectionRotationChanged;

        playerAnimator.OnCrossfadeFinished += HandleOnPlayerAnimatorCrossfadeFinished;*/

        /*playerSpawnItemOnHand.OnItemOnHandSpawned += HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned += HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected += OnPlayerSpawnItemOnHandItemSocketSelected;*/

        /*playerGetUp.OnPlayerGetUp += HandleOnPlayerGetUp;
        playerRagdollEnabler.OnRagdollDisabled += HandleOnRagdollDisabled;*/
    }

    private void UnHandleEvents()
    {

        playerInventory.OnItemAdded -= HandleOnItemAdded;
        playerInventory.OnItemChanged -= HandleOnItemChanged;
        /*playerInventory.OnItemSelected -= HandleOnItemSelected;
        playerInventory.OnItemSelectedSO -= HandleOnItemSelectedSO;*/

        playerLauncher.OnItemLaunched -= HandleOnItemLaunched;

        /*if (playerStateMachine != null)
            playerStateMachine.OnStateChanged -= HandleOnStateChanged;*/

        playerDragController.OnDragStart -= HandleOnDragStart;
        playerDragController.OnDragChange -= HandleOnDragChange;
        playerDragController.OnDragCancelable -= HandleOnDragCancelable;

        playerInventoryUI.OnItemSelectedByUI -= HandleOnItemSelectedByUI;

        playerDetectFacingDirection.OnRotationChanged -= HandleOnPlayerDetectFacingDirectionRotationChanged;

        playerAnimator.OnCrossfadeFinished -= HandleOnPlayerAnimatorCrossfadeFinished;

        /*playerSpawnItemOnHand.OnItemOnHandSpawned -= HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned -= HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected -= OnPlayerSpawnItemOnHandItemSocketSelected;*/

        playerGetUp.OnPlayerGetUp -= HandleOnPlayerGetUp;
        playerRagdollEnabler.OnRagdollDisabled -= HandleOnRagdollDisabled;

        cameraManager.UnInitializeOwner();
        playerLauncher.UnInitializeOwner();
        playerInventoryUI.UnHandleInitializeOwner();
    }
    
    private void HandleOnDragRelease()
    {
       //Used to the Owner tells the client that the drag has been released and its time to Lerp the aim position and spawn item
       playerRotateToAim.SyncAimPosition(playerRotateToAim.AimTransform.position, () =>
       {
           //Finished Lerp Aim Position
           playerDragController.TriggerOnDragReleaseServerRpc();
       });
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

    private void HandleOnItemSelectedByUI(int itemID)
    {
        playerInventory.SelectItemDataByItemInventoryID(itemID);
        Debug.Log($"HandleOnItemSelectedByUI - Item ID: {itemID}");
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
        playerJumpUI.HandleOnPlayerInventoryItemSelected(selectedItemInventoryIndex);

        playerSpawnItemOnHand.HandleOnPlayerInventoryItemSelected(selectedItemInventoryIndex);
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

    private void HandleOnItemCallbackAction()
    {
        playerGetUp.HandleOnItemCallbackAction();
    }

    private void HandleOnRagdollDisabled()
    {
        playerDetectFacingDirection.FaceOtherPlayer();
        playerRotateToAim.OnRagdollDisabled();
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
        /*playerStateMachine.TransitionTo(playerStateMachine.myTurnEndedState);
        TransitionToMyTurnEndedStateServerRpc();*/
    }

    private void GameFlowManager_OnMyTurnStarted()
    {
        // My Turn Started, I can play
        ChangePlayerState(PlayerState.MyTurnStarted);
        /*playerStateMachine.TransitionTo(playerStateMachine.myTurnStartedState);
        TransitionToMyTurnStartedStateServerRpc();*/
    }
    
    //STATES
    
    /// <summary>
    /// Call this to change the player state. Will sync automatically
    /// </summary>
    /// <param name="playerState"> The Next State</param>
    public void ChangePlayerState(PlayerState playerState)
    {
        if (playerStateMachine == null)
        {
            Debug.LogWarning("Player State Machine is null, cannot change state.");
            return;
        }
        
        Debug.Log($"PlayerThrower - Changing Player State to: {playerState} - Old State Was: {playerStateMachine.CurrentState}");
        playerStateMachine.ChangeStateWithPlayerState(playerState);
        TransitionToStateServerRpc(playerState);
    }
    
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void TransitionToStateServerRpc(PlayerState playerState)
    {
        TransitionToStateClientRpc(playerState);
    }

    [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Reliable)]
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
        GameManager.OnClientOwnershipChanged -= HandleOnClientOwnershipChanged;
        thisPlayableState.OnValueChanged -= PlayableStateInitialize;


        if (IsOwner)
        {
            turnManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;

            turnManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;

            turnManager.OnMyTurnJumped -= GameFlowManager_OnMyTurnJumped;

            gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChanged;

            BaseItemThrowable.OnItemCallbackAction -= HandleOnItemCallbackAction;

            hitReceiveNetworked.OnHitReceive -= HandleOnHitReceive;

            UnHandleEvents();
        }
        
        playerSpawnItemOnHand.OnItemOnHandSpawned -= HandleOnPlayerSpawnItemOnHandItemOnHandSpawned;
        playerSpawnItemOnHand.OnItemOnHandDespawned -= HandleOnPlayerSpawnItemOnHandItemOnHandDespawned;
        playerSpawnItemOnHand.OnItemSocketSelected -= OnPlayerSpawnItemOnHandItemSocketSelected;
        
        playerInventory.OnItemSelected -= HandleOnItemSelected;
        playerInventory.OnItemSelectedSO -= HandleOnItemSelectedSO;
        
        if (playerStateMachine != null)
            playerStateMachine.OnStateChanged -= HandleOnStateChanged;
    }

    public override void OnLostOwnership()
    {
        playerTouchColl.enabled = false;
        UnHandleEvents();
    }
    
}
