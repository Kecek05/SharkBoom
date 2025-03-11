using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerLauncher playerLauncher;
    [SerializeField] private Collider playerTouchColl;
    [SerializeField] private GameObject[] playerColliders;
    private PlayerStateMachine playerStateMachine;

    private NetworkVariable<GameFlowManager.PlayableState> thisPlayableState = new();

    //Publics
    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerInventoryUI PlayerInventoryUI => playerInventoryUI;
    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerDragController PlayerDragController => playerDragController;
    public PlayerLauncher PlayerLauncher => playerLauncher;

    public override void OnNetworkSpawn()
    {
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 100).ToString();

        thisPlayableState.OnValueChanged += PlayableStateChanged;
        PlayableStateChanged(thisPlayableState.Value, thisPlayableState.Value);

        if (IsOwner)
        {

            CameraManager.Instance.SetPlayer(this);
            GameFlowManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

            

            playerStateMachine = new PlayerStateMachine(this);

            playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);

        } else
        {
            //if not owner, turn off touch collider
            playerTouchColl.enabled = false;

        }

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
        GameFlowManager.Instance.PlayerPlayedRpc(GameFlowManager.Instance.LocalplayableState);

    }


    [Rpc(SendTo.Server)]
    public void SetThisPlayableStateRpc(GameFlowManager.PlayableState playableState)
    {
        // Cant be OnnetworkSpawn because it needs to be called by NetworkServer
        thisPlayableState.Value = playableState;

    }
    

    private void PlayableStateChanged(GameFlowManager.PlayableState previousValue, GameFlowManager.PlayableState newValue)
    {
        if (IsOwner)
        {
            GameFlowManager.Instance.SetLocalStates(thisPlayableState.Value); //pass to GameFlow to know when its local turn
        }

        if (thisPlayableState.Value == GameFlowManager.PlayableState.Player1Playing)
        {

            foreach(GameObject playerCollider in playerColliders)
            {
                playerCollider.layer = GameFlowManager.PLAYER_1_LAYER;
            }
        }
        else
        {
            foreach (GameObject playerCollider in playerColliders)
            {
                playerCollider.layer = GameFlowManager.PLAYER_2_LAYER;
            }
        }


    }

    public override void OnNetworkDespawn()
    {
        thisPlayableState.OnValueChanged -= PlayableStateChanged;

        if (IsOwner)
        {
            GameFlowManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;
        }
    }

}
