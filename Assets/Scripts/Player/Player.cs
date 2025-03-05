using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public event Action OnPlayerReady;

    [BetterHeader("References")]

    [SerializeField] private PlayerInventory playerInventory;
    public PlayerInventory PlayerInventory => playerInventory;

    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    public PlayerInventoryUI PlayerInventoryUI => playerInventoryUI;

    [SerializeField] private DragAndShoot playerDragAndShoot;
    public DragAndShoot PlayerDragAndShoot => playerDragAndShoot;


    public override void OnNetworkSpawn()
    {
        gameObject.name = "Player " + UnityEngine.Random.Range(0, 100).ToString();
    }

    public void SetPlayerReady() //UI Button will call this
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
