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


    public void Launch() //Called by the script on animator
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

        SpawnProjectile(itemLauncherData); 
    
        OnItemLaunched?.Invoke(playerInventory.SelectedItemInventoryIndex); //pass itemInventoryIndex
         
    }

    private void SpawnProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {
        if (playerInventory.GetItemSOByItemSOIndex(launcherData.selectedItemSOIndex).itemPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {launcherData.selectedItemSOIndex} has no client prefab");
            return;
        }

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(launcherData);
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
