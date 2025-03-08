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

    private bool playerCanJumpThisTurn = false;
    private bool playerCanShootThisTurn = false;

    private PlayerStateMachine playerStateMachine;

    //Publics

    public PlayerStateMachine PlayerStateMachine => playerStateMachine;
    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerInventoryUI PlayerInventoryUI => playerInventoryUI;
    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerDragController PlayerDragController => playerDragController;
    public PlayerLauncher PlayerLauncher => playerLauncher;
    public bool PlayerCanJumpThisTurn => playerCanJumpThisTurn;
    public bool PlayerCanShootThisTurn => playerCanShootThisTurn;

    public override void OnNetworkSpawn()
    {
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 100).ToString();

        if(IsOwner)
        {
            GameFlowManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

            playerStateMachine = new PlayerStateMachine(this);

            playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);
        }
    }



    private void GameFlowManager_OnMyTurnStarted()
    {
        // My Turn Started, I can play
        playerStateMachine.TransitionTo(playerStateMachine.myTurnStartedState);
        Debug.Log("I can play!");

    }

    public void SetPlayerCanJumpThisTurn(bool canJump)
    {
        playerCanJumpThisTurn = canJump;
    }

    public void SetPlayerCanShootThisTurn(bool canShoot)
    {
        playerCanShootThisTurn = canShoot;
    }


    //DEBUG
    [Command("player-passTurn")]
    private void PassTurn()
    {
        playerStateMachine.TransitionTo(playerStateMachine.idleEnemyTurnState);
        GameFlowManager.Instance.PlayerPlayedRpc(GameFlowManager.Instance.LocalplayableState);

    }


    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameFlowManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;
        }
    }
}
