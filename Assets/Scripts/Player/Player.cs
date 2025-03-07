using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public event Action OnPlayerReady;
    public event Action OnPlayerCanPlay;
    public event Action OnPlayerCantPlay;
    public event Action OnPlayerJumped;
    public event Action OnPlayerShooted;

    [BetterHeader("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerLauncher playerLauncher;

    private bool playerCanJumpThisTurn = false;
    private bool playerCanShootThisTurn = false;


    //Publics
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
            GameFlowManager.OnMyTurnEnded += GameFlowManager_OnMyTurnEnded;
        }
    }



    private void GameFlowManager_OnMyTurnStarted()
    {
        // My Turn Started, I can play
        SetPlayerCanJumpThisTurn(true);
        SetPlayerCanShootThisTurn(true);

        OnPlayerCanPlay?.Invoke();

        Debug.Log("I can play!");
    }

    private void GameFlowManager_OnMyTurnEnded()
    {
        //My Turn Ended, I cant play


        OnPlayerCantPlay?.Invoke();

        Debug.Log("I cant play!");
    }


    public void PlayerJumped()
    {
        SetPlayerCanJumpThisTurn(false);

        OnPlayerJumped?.Invoke();
    }

    public void PlayerShooted()
    {
        //Player Shooted, cant do any thing else. Round ended
        SetPlayerCanJumpThisTurn(false);
        SetPlayerCanShootThisTurn(false);

        OnPlayerShooted?.Invoke();

        GameFlowManager.Instance.PlayerPlayedRpc(GameFlowManager.Instance.LocalplayableState);
        //This player round ended, wait for the item to finish their action an then change the game state to the next player
        // GameFlowManager.Instance.SetGameStateRpc(GameFlowManager.Instance.LocalplayedState);
    }


    public void SetPlayerReady()
    {
        if(playerInventory.GetSelectedItemSO() == null)
        {
            Debug.LogWarning("Item was not selected");
            return;
        }


        GameFlowManager.Instance.SetPlayerReadyServerRpc();

        OnPlayerReady?.Invoke();

        Debug.Log("Player Setted to Ready");
    }

    private void SetPlayerCanJumpThisTurn(bool canJump)
    {
        playerCanJumpThisTurn = canJump;
    }

    private void SetPlayerCanShootThisTurn(bool canShoot)
    {
        playerCanShootThisTurn = canShoot;
    }


    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameFlowManager.OnMyTurnStarted -= GameFlowManager_OnMyTurnStarted;
            GameFlowManager.OnMyTurnEnded -= GameFlowManager_OnMyTurnEnded;
        }
    }
}
