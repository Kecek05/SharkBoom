using QFSW.QC;
using Sortify;
using System;
using System.Collections;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLauncher : NetworkBehaviour
{

    /// <summary>
    /// item launched, pass itemID
    /// </summary>
    public event Action<int> OnItemLaunched;
    
    /// <summary>
    /// Called when the last item used was synced. It will trigger all the chain reaction to throw an item.
    /// Pass the Position of the Aim.
    /// </summary>
    public event Action<Vector3> OnLastItemSynced;
    
    [BetterHeader("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerThrower playerThrower;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    [SerializeField] private PlayerRotateToAim playerRotateToAim;
    
    //private BaseItemActivableManager itemActivableManager;
    private BaseItemThrowable lastProjectile;
    private BaseItemThrowableActivable lastItemThrowableActivable;
    private AutoActivateItemComponent lastAutoActivateItemComponent;
    private BaseTimerManager timerManager;

    private ItemLauncherData lastItemLauncherData;
    
    /// <summary>
    /// Threshhold, in units, to activate the reconciliation
    /// </summary>
    private float distanceThreshold = 2f;
    

    private bool canActivateItem = true;
    public void InitializeOwner()
    {
        //itemActivableManager = ServiceLocator.Get<BaseItemActivableManager>();
        timerManager = ServiceLocator.Get<BaseTimerManager>();

        inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        if(!IsOwner) return; //To be shure that only the owner can activate items
        if(context.started && canActivateItem)
        {
            canActivateItem = false;
            ActivateItem();
        }
    }

    private void ActivateItem()
    {
        if (lastItemThrowableActivable != null)
        {
            lastItemThrowableActivable.TryActivate();
            TriggerUseItemOnServerRpc(lastItemThrowableActivable.GetReconcileData());
        }
    }

    private void AutoActivateItem()
    {
        if (lastAutoActivateItemComponent)
        {
            lastAutoActivateItemComponent.OnActivate -= AutoActivateItem;
            lastAutoActivateItemComponent = null;
            ActivateItem();
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void TriggerUseItemOnServerRpc(ItemReconcileData reconcileData)
    {
        TriggerUseItemOnClientRpc(reconcileData);
    }
    
    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    public void TriggerUseItemOnClientRpc(ItemReconcileData reconcileData)
    {
        StartCoroutine(WaitForCorrectPositionToActivate(reconcileData));
    }
    
    private IEnumerator WaitForCorrectPositionToActivate(ItemReconcileData reconcileData)
    {
        while (!lastItemThrowableActivable)
        {
            yield return null;
        }
        //Item spawned
        while (Vector3.Distance(lastItemThrowableActivable.transform.position, reconcileData.position) > distanceThreshold)
        {
            yield return null; // Wait until the item is in the correct position
        }
        //Item is in the correct position, Reconcile it
        lastItemThrowableActivable.Reconcile(reconcileData);
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {
        if(!IsOwner) return;
        
        if (state == PlayerState.DragReleaseJump || state == PlayerState.DragReleaseItem)
        {
            // Released, pause timer
            timerManager.TogglePauseTimer(true);
        }
    }

    public void HandleOnItemOnHandSpawned(BaseItemThrowable throwable)
    {
        lastProjectile = throwable;
        Debug.Log($"ITEM SPAWNED ON HAND {lastProjectile.gameObject.name}");
    }

    public void HandleOnItemOnHandDespawned(BaseItemThrowable throwable)
    {
        lastProjectile = null;
        // if (throwable == lastProjectile)
        // {
        //     lastProjectile = null;
        // }
    }
    
    public void Launch() //Called by the script on animator
    {
        if (IsOwner)
        {
            canActivateItem = true;
            //itemActivableManager.ResetItemActivable(); //Only owner can activate the item.
            
            ItemLauncherData itemLauncherData = new ItemLauncherData
            {
                dragForce = playerDragController.DragForce, 
                dragDirection = playerDragController.DirectionOfDrag,
                selectedItemID = playerInventory.SelectedItemID, 
                ownerPlayableState = playerThrower.ThisPlayableState.Value,
                shootPosition = lastProjectile.transform.position,
                shootRotation = lastProjectile.transform.rotation
            };
            
            lastItemLauncherData = itemLauncherData;
            
            SyncItemLauncherDataServerRpc(lastItemLauncherData, playerRotateToAim.AimTransform.position);
            
            Debug.Log($"STEPS OWNER 1 - OWNER CREATED LAUNCHER DATA - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Position: {lastItemLauncherData.shootPosition} - Rotation: {lastItemLauncherData.shootRotation} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        }
        
        SpawnProjectile(lastItemLauncherData); 
         Debug.Log($"STEPS LAST - ITEM LAUNCHED - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Position: {lastItemLauncherData.shootPosition} - Rotation: {lastItemLauncherData.shootRotation} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        //SpawnProjectileServerRpc(itemLauncherData);
        //Debug.Log($"Item Launcher - Launching item with ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState}");
        
        OnItemLaunched?.Invoke(playerInventory.SelectedItemID); //pass itemInventoryIndex
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void SyncItemLauncherDataServerRpc(ItemLauncherData itemLauncherData, Vector3 aimPos)
    {
        SyncItemLauncherDataClientRpc(itemLauncherData, aimPos);
    }

    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void SyncItemLauncherDataClientRpc(ItemLauncherData itemLauncherData, Vector3 aimPos)
    {
        lastItemLauncherData = itemLauncherData;
         Debug.Log($"STEPS CLIENT 1 - ITEM LAUNCHER DATA SYNCED - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        OnLastItemSynced?.Invoke(aimPos);
    }

    // [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    // private void SpawnProjectileServerRpc(ItemLauncherData itemLauncherData)
    // {
    //     SpawnProjectileClientRpc(itemLauncherData);
    // }
    //
    // [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    // private void SpawnProjectileClientRpc(ItemLauncherData itemLauncherData)
    // {
    //     SpawnProjectile(itemLauncherData);
    // }
    
    private void SpawnProjectile(ItemLauncherData launcherData)
    {
        StartCoroutine(WaitForProjectileAndSpawn(launcherData));
    }

    private IEnumerator WaitForProjectileAndSpawn(ItemLauncherData launcherData)
    {
        // Wait for lastProjectile to be assigned
        while (!lastProjectile)
        {
            yield return null;
        }

        if (!playerInventory.GetItemSOByItemID(launcherData.selectedItemID).itemPrefab)
        {
            // Debug.LogWarning($"Player Launcher - ItemSOIndex: {launcherData.selectedItemID} has no client prefab");
            yield break;
        }

        // Debug.Log($"SPAWNING PROJECTILE - SETTING POSITION - LAST POSITION: {lastProjectile.transform.position} - NEW POSITION: {launcherData.shootPosition} - LAST ROTATION: {lastProjectile.transform.rotation} NEW ROTATION: {launcherData.shootRotation}");

        lastProjectile.transform.position = lastItemLauncherData.shootPosition;
        lastProjectile.transform.rotation = lastItemLauncherData.shootRotation;

        if (lastProjectile.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(launcherData, IsOwner);
            Debug.Log($"ITEM RELEASED - {itemThrowable.gameObject.name}");
            // Debug.Log($"Player Launcher - Item released: {itemThrowable.name}");
        }

        if (lastProjectile.TryGetComponent(out BaseItemThrowableActivable activable))
        {
            lastItemThrowableActivable = activable;
            if (lastItemThrowableActivable.TryGetComponent(out AutoActivateItemComponent autoActivate))
            {
                lastAutoActivateItemComponent = autoActivate;
                lastAutoActivateItemComponent.OnActivate += AutoActivateItem;
            }
        }
        else
        {
            lastItemThrowableActivable = null;
        }
    }

    // private void SpawnProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    // {
    //     if (!playerInventory.GetItemSOByItemID(launcherData.selectedItemID).itemPrefab)
    //     {
    //         Debug.LogWarning($"Player Launcher - ItemSOIndex: {launcherData.selectedItemID} has no client prefab");
    //         return;
    //     }
    //     
    //     Debug.Log($"SPAWNING PROJECTILE - SETTING POSITION - LAST POSITION: {lastProjectile.transform.position} - NEW POSITION: {launcherData.shootPosition} - LAST ROTATION: {lastProjectile.transform.rotation} NEW ROTATION: {launcherData.shootRotation}");
    //     lastProjectile.transform.position = lastItemLauncherData.shootPosition;
    //     lastProjectile.transform.rotation = lastItemLauncherData.shootRotation;
    //
    //     if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
    //     {
    //         itemThrowable.ItemReleased(launcherData);
    //         Debug.Log($"Player Launcher - Item released: {itemThrowable.name}");
    //     }
    //
    //     if (lastProjectile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
    //     {
    //         //Get the ref to active the item
    //         itemActivableManager.SetItemThrowableActivableClient(activable);
    //     }
    // }

    public void UnInitializeOwner()
    {
        inputReader.OnTouchPressEvent -= InputReader_OnTouchPressEvent;
    }

}
