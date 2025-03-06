using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public event Action OnPlayerReady;

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


}
