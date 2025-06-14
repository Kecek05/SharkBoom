using QFSW.QC;
using Sortify;
using System;
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
    /// </summary>
    public event Action OnLastItemSynced;
    
    [BetterHeader("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerDragController playerDragController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerThrower playerThrower;
    [SerializeField] private PlayerSpawnItemOnHand playerSpawnItemOnHand;
    
    private BaseItemActivableManager itemActivableManager;
    private BaseItemThrowable lastProjectile;
    private BaseTimerManager timerManager;

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        itemActivableManager = ServiceLocator.Get<BaseItemActivableManager>();
        timerManager = ServiceLocator.Get<BaseTimerManager>();

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
    }

    public void HandleOnItemOnHandDespawned(BaseItemThrowable throwable)
    {
        if (throwable == lastProjectile)
        {
            lastProjectile = null;
        }
    }
    
    private float lastDragForce = 0f;
    private Vector3 lastDragDirection = Vector3.zero;
    private int lastItemID = 0;
    private PlayableState lastPlayableState = PlayableState.None;

    private ItemLauncherData lastItemLauncherData;
    
    public void Launch() //Called by the script on animator
    {
        if (IsOwner)
        {
            itemActivableManager.ResetItemActivable(); //Only owner can activate the item.
            
            ItemLauncherData itemLauncherData = new ItemLauncherData
            {
                dragForce = playerDragController.DragForce, 
                dragDirection = playerDragController.DirectionOfDrag,
                selectedItemID = playerInventory.SelectedItemID, 
                ownerPlayableState = playerThrower.ThisPlayableState.Value,
            };
            
            lastItemLauncherData = itemLauncherData;
            
            SyncItemLauncherDataServerRpc(lastItemLauncherData);
            
            Debug.Log($"STEPS OWNER 1 - OWNER CREATED LAUNCHER DATA - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        }
        
        SpawnProjectile(lastItemLauncherData); 
        Debug.Log($"STEPS LAST - ITEM LAUNCHED - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        //SpawnProjectileServerRpc(itemLauncherData);
        //Debug.Log($"Item Launcher - Launching item with ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState}");
        
        OnItemLaunched?.Invoke(playerInventory.SelectedItemID); //pass itemInventoryIndex
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void SyncItemLauncherDataServerRpc(ItemLauncherData itemLauncherData)
    {
        SyncItemLauncherDataClientRpc(itemLauncherData);
    }

    [Rpc(SendTo.NotOwner, Delivery = RpcDelivery.Reliable)]
    private void SyncItemLauncherDataClientRpc(ItemLauncherData itemLauncherData)
    {
        lastItemLauncherData = itemLauncherData;
        Debug.Log($"STEPS CLIENT 1 - ITEM LAUNCHER DATA SYNCED - Item ID: {lastItemLauncherData.selectedItemID}, Force: {lastItemLauncherData.dragForce}, Direction: {lastItemLauncherData.dragDirection} - Owner: {lastItemLauncherData.ownerPlayableState} - {gameObject.name}");
        OnLastItemSynced?.Invoke();
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

    private void SpawnProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {
        if (!playerInventory.GetItemSOByItemID(launcherData.selectedItemID).itemPrefab)
        {
            Debug.LogWarning($"Player Launcher - ItemSOIndex: {launcherData.selectedItemID} has no client prefab");
            return;
        }

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(launcherData);
            Debug.Log($"Player Launcher - Item released: {itemThrowable.name}");
        }

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
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
