using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLauncher : NetworkBehaviour
{

    /// <summary>
    /// item launched, pass itemInventoryIndex
    /// </summary>
    public event Action<int> OnItemLaunched; 


    [BetterHeader("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform spawnItemPos;
    [SerializeField] private PlayerThrower player;

    //private BaseItemThrowableActivable itemThrowableActivableClient;
    //private BaseItemThrowableActivable itemThrowableActivableServer;
    private BaseItemActivableManager itemActivableManager;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

            inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;

            itemActivableManager = ServiceLocator.Get<BaseItemActivableManager>();
        }
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        //Debug.Log($"Debug Touch itemActivated: {itemActivated} ItemActivable: {itemActivable}");

        if(context.started && (itemActivableManager.ItemThrowableActivableClient != null || itemActivableManager.ItemThrowableActivableServer != null))
        {
            itemActivableManager.UseItem();
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState state)
    {

        if (state == player.PlayerStateMachine.dragReleaseJump || state == player.PlayerStateMachine.dragReleaseItem)
        {
            Launch();
        }
    }

    private void Launch()
    {

        itemActivableManager.ResetItemActivable();


        ItemLauncherData itemLauncherData = new ItemLauncherData
        {
            dragForce = player.PlayerDragController.DragForce, 
            dragDirection = player.PlayerDragController.DirectionOfDrag,
            selectedItemSOIndex = player.PlayerInventory.GetSelectedItemSOIndex(), 
            ownerPlayableState = ServiceLocator.Get<BaseTurnManager>().LocalPlayableState,
        };

        SpawnProjectileServerRpc(itemLauncherData); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(itemLauncherData); //Spawn fake projectile on client
    
        OnItemLaunched?.Invoke(player.PlayerInventory.SelectedItemInventoryIndex); //pass itemInventoryIndex
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(ItemLauncherData launcherData) // on server, need to pass the prefab for the other clients instantiate it
    {
        if(player.PlayerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemServerPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {launcherData.selectedItemSOIndex} has no server prefab");
            return;
        }
        
        
        GameObject projetctile = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemServerPrefab, spawnItemPos.position, Quaternion.identity);


        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(launcherData);
        }

        if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        {
            //Get the ref to active the item
            itemActivableManager.SetItemThrowableActivableServer(activable);
        }


        SpawnProjectileClientRpc(launcherData);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientRpc(ItemLauncherData launcherData) //pass info to other clients
    {
        if (IsOwner) return; // already spawned

        SpawnDummyProjectile(launcherData);

    }

    private void SpawnDummyProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {
        if (player.PlayerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemClientPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {launcherData.selectedItemSOIndex} has no client prefab");
            return;
        }


        GameObject projetctile = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemClientPrefab, spawnItemPos.position, Quaternion.identity);


        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(launcherData);
        }

        if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        {
            //Get the ref to active the item
            itemActivableManager.SetItemThrowableActivableClient(activable);
        }

    }

    public override void OnNetworkDespawn()
    {
        if(IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;

            inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
        }
    }

}
