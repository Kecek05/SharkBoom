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
    private PlayerStateMachine playerStateMachine;

    [BetterHeader("Settings")]
    [SerializeField] private int player1Layer;
    [SerializeField] private int player2Layer;
    private GameFlowManager.PlayableState thisPlayableState;

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
       

        if (IsOwner)
        {
            CameraManager.Instance.SetPlayer(this);
            GameFlowManager.OnMyTurnStarted += GameFlowManager_OnMyTurnStarted;

            playerStateMachine = new PlayerStateMachine(this);

            playerStateMachine.Initialize(playerStateMachine.idleEnemyTurnState);

            GameFlowManager.Instance.SetLocalStates(thisPlayableState);

        } else
        {
            //if not owner, turn off touch collider
            playerTouchColl.enabled = false;
        }

        if (thisPlayableState == GameFlowManager.PlayableState.Player1Playing)
        {
            gameObject.layer = player1Layer;
        }
        else
        {
            gameObject.layer = player2Layer;
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


    public void SetThisPlayableState(GameFlowManager.PlayableState playableState)
    {
        thisPlayableState = playableState;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameFlowManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;
        }
    }

}
