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
    [SerializeField] private BoxCollider playerTouchColl;
    [SerializeField] private GameObject[] playerColliders;
    private PlayerStateMachine playerStateMachine;

    private NetworkVariable<PlayableState> thisPlayableState = new();

    private BaseTurnManager turnManager;
    private BaseGameStateManager gameStateManager;

    //Publics
    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public NetworkVariable<PlayableState> ThisPlayableState => thisPlayableState;

    public override void OnNetworkSpawn()
    {
        gameStateManager = ServiceLocator.Get<BaseGameStateManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        thisPlayableState.OnValueChanged += PlayableStateInitialize;

        if(!IsHost) // host will add itself twice
            PlayableStateInitialize(thisPlayableState.Value, thisPlayableState.Value);

        if (IsOwner)
        {
            InitializeOwner();
        } else
        {
            //if not owner, turn off touch collider
            playerTouchColl.enabled = false;
        }

        HandleEvents();
    }

    private void InitializeOwner()
    {
        //Owner initialize code
        turnManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

        turnManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;

        turnManager.OnMyTurnJumped += GameFlowManager_OnMyTurnJumped;

        gameStateManager.CurrentGameState.OnValueChanged += HandleOnGameStateChanged;

        playerStateMachine = new PlayerStateMachine(this, playerDragController, playerInventory);

        playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);

        playerFlipGfx.InitializeOwner();
        playerRotateToAim.InitializeOwner();
        playerInventory.InitializeOwner();
        playerLauncher.InitializeOwner();
        playerDragController.InitializeOwner(playerInventory.GetItemSOByItemSOIndex(0).rb);
    }

    private void HandleEvents()
    {
        playerInventory.OnItemAdded += HandleOnItemAdded;
        playerInventory.OnItemChanged += HandleOnItemChanged;
        playerInventory.OnItemSelected += HandleOnItemSelected;
        playerInventory.OnItemSelectedSO += HandleOnItemSelectedSO;

        playerLauncher.OnItemLaunched += HandleOnItemLaunched;

        if(IsOwner)
            playerStateMachine.OnStateChanged += HandleOnStateChanged;

        playerDragController.OnDragStart += HandleOnDragStart;
        playerDragController.OnDragChange += HandleOnDragChange;
        playerDragController.OnDragCancelable += HandleOnDragCancelable;

        playerInventoryUI.OnItemSelectedByUI += HandleOnItemSelectedByUI;
    }


    private void UnHandleEvents()
    {
        playerInventory.OnItemAdded -= HandleOnItemAdded;
        playerInventory.OnItemChanged -= HandleOnItemChanged;
        playerInventory.OnItemSelected -= HandleOnItemSelected;
        playerInventory.OnItemSelectedSO -= HandleOnItemSelectedSO;

        playerLauncher.OnItemLaunched -= HandleOnItemLaunched;

        if (IsOwner)
            playerStateMachine.OnStateChanged -= HandleOnStateChanged;

        playerDragController.OnDragStart -= HandleOnDragStart;
        playerDragController.OnDragChange -= HandleOnDragChange;
        playerDragController.OnDragCancelable -= HandleOnDragCancelable;

        playerInventoryUI.OnItemSelectedByUI -= HandleOnItemSelectedByUI;
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
        playerInventoryUI.UpdateOpenInventoryButton(itemSOSelected.itemIcon);
    }

    private void HandleOnItemSelectedByUI(int itemInventoryIndex)
    {
        playerInventory.SelectItemDataByItemInventoryIndex(itemInventoryIndex);
    }

    private void HandleOnDragChange(float forcePercent, float angle)
    {
        playerFlipGfx.HandleOnPlayerDragControllerDragChange(forcePercent, angle);
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
        playerFlipGfx.HandleOnPlayerStateMachineStateChanged(state);
        playerDragUi.HandleOnPlayerStateMachineStateChanged(state);

        playerInventoryUI.HandleOnPlayerStateMachineStateChanged(state);
    }

    private void HandleOnItemLaunched(int itemInventoryIndex)
    {
        playerInventory.HandleOnPlayerLauncherItemLaunched(itemInventoryIndex);
    }

    private void HandleOnItemSelected(int selectedItemInventoryIndex)
    {
        playerDragController.SetDragRb(playerInventory.GetSelectedItemSO().rb);

        playerInventoryUI.HandleOnPlayerInventoryItemSelected(selectedItemInventoryIndex);
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
        Debug.Log($"Server is the Owner? {IsOwnedByServer}");

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
            ServiceLocator.Get<BaseTurnManager>().InitializeLocalStates(newValue); //pass to GameFlow to know when its local turn
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
        thisPlayableState.OnValueChanged -= PlayableStateInitialize;

        if (IsOwner)
        {
            turnManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;

            turnManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;

            turnManager.OnMyTurnJumped -= GameFlowManager_OnMyTurnJumped;

            gameStateManager.CurrentGameState.OnValueChanged -= HandleOnGameStateChanged;
        }

        UnHandleEvents();

    }

    public override void OnGainedOwnership()
    {
        Debug.Log($"Gained Ownership, new owner is: {OwnerClientId}");
    }

    public override void OnLostOwnership()
    {
        Debug.Log($"Lost Ownership");
    }
}
