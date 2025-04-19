using QFSW.QC;
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
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerInventory playerInventory;
    
    private BaseItemActivableManager itemActivableManager;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        itemActivableManager = ServiceLocator.Get<BaseItemActivableManager>();

        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {

        if(context.started && (itemActivableManager.ItemThrowableActivableClient != null || itemActivableManager.ItemThrowableActivableServer != null))
        {
            itemActivableManager.UseItem();
        }
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {

        if (state == PlayerState.DragReleaseJump || state == PlayerState.DragReleaseItem)
        {
            //Launch();
        }
    }


    public void Launch()
    {
        if (!IsOwner) return;

        itemActivableManager.ResetItemActivable();


        ItemLauncherData itemLauncherData = new ItemLauncherData
        {
            dragForce = playerDragController.DragForce, 
            dragDirection = playerDragController.DirectionOfDrag,
            selectedItemSOIndex = playerInventory.GetSelectedItemSOIndex(), 
            ownerPlayableState = ServiceLocator.Get<BaseTurnManager>().LocalPlayableState,
        };

        SpawnProjectileServerRpc(itemLauncherData); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(itemLauncherData); //Spawn fake projectile on client
    
        OnItemLaunched?.Invoke(playerInventory.SelectedItemInventoryIndex); //pass itemInventoryIndex
         
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(ItemLauncherData launcherData) // on server, need to pass the prefab for the other clients instantiate it
    {
        if(playerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemServerPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {launcherData.selectedItemSOIndex} has no server prefab");
            return;
        }
        
        
        GameObject projetctile = Instantiate(playerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemServerPrefab, spawnItemPos.position, Quaternion.identity);


        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(launcherData);
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
        if (playerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemClientPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {launcherData.selectedItemSOIndex} has no client prefab");
            return;
        }


        GameObject projetctile = Instantiate(playerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemClientPrefab, spawnItemPos.position, Quaternion.identity);


        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(launcherData);
        }

        if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        {
            //Get the ref to active the item
            itemActivableManager.SetItemThrowableActivableClient(activable);
        }

    }

    public void UnInitializeOwner()
    {
        if(!IsOwner) return;

        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
    }

}
