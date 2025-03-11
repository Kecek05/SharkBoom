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
    [SerializeField] private Player player;

    //private BaseItemThrowableActivable itemThrowableActivableClient;
    //private BaseItemThrowableActivable itemThrowableActivableServer;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

            inputReader.OnTouchPressEvent += InputReader_OnTouchPressEvent;
        }
    }

    private void InputReader_OnTouchPressEvent(InputAction.CallbackContext context)
    {
        //Debug.Log($"Debug Touch itemActivated: {itemActivated} ItemActivable: {itemActivable}");

        if(context.started && (ItemActivableManager.Instance.ItemThrowableActivableClient != null || ItemActivableManager.Instance.ItemThrowableActivableServer != null))
        {
            ItemActivableManager.Instance.UseItem();
            //if(itemThrowableActivableClient != null)
            //    itemThrowableActivableClient.TryActivate();

            //if (itemThrowableActivableServer != null)
            //    itemThrowableActivableServer.TryActivate();
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
        //itemThrowableActivableClient = null; //reset value
        //itemThrowableActivableServer = null;
        ItemActivableManager.Instance.ResetItemActivable();

        SpawnProjectileServerRpc(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag, player.PlayerInventory.GetSelectedItemSOIndex(), GameFlowManager.Instance.LocalplayableState); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag, player.PlayerInventory.GetSelectedItemSOIndex(), GameFlowManager.Instance.LocalplayableState); //Spawn fake projectile on client
    
        OnItemLaunched?.Invoke(player.PlayerInventory.SelectedItemInventoryIndex); //pass itemInventoryIndex
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(float dragForce, Vector3 dragDirection, int selectedItemSOIndex, PlayableState ownerPlayableState) // on server, need to pass the prefab for the other clients instantiate it
    {
        if(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemServerPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {selectedItemSOIndex} has no server prefab");
            return;
        }
        
        
        GameObject projetctile = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemServerPrefab, spawnItemPos.position, Quaternion.identity);


        //Collider[] projectileColliders = projetctile.GetComponentsInChildren<Collider>();

        //if(projectileColliders.Length > 0)
        //{
        //    for(int i = 0; i < projectileColliders.Length; i++)
        //    {
        //        foreach (Collider playerCollider in playerColliders)
        //        {
        //            Physics.IgnoreCollision(playerCollider, projectileColliders[i]); // Ignore collision between the player and the projectile
        //        }
        //    }
        //}

        if(projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(ownerPlayableState);
        }

        if (projetctile.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection); //Call interface
        }

        if (projetctile.transform.TryGetComponent(out IFollowable followable))
        {
            followable.Follow(transform); //Call interface
        }

        if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        {

           ItemActivableManager.Instance.SetItemThrowableActivableServer(activable);
        }



        SpawnProjectileClientRpc(dragForce, dragDirection, selectedItemSOIndex, ownerPlayableState);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientRpc(float dragForce, Vector3 dragDirection, int selectedItemSOIndex, PlayableState ownerPlayableState) //pass info to other clients
    {
        if (IsOwner) return; // already spawned

        SpawnDummyProjectile(dragForce, dragDirection, selectedItemSOIndex, ownerPlayableState);

    }

    private void SpawnDummyProjectile(float dragForce, Vector3 dragDirection, int selectedItemSOIndex, PlayableState ownerPlayableState) // on client, need to pass the prefab for the other clients instantiate it
    {
        if (player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemClientPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {selectedItemSOIndex} has no client prefab");
            return;
        }


        GameObject projetctile = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemClientPrefab, spawnItemPos.position, Quaternion.identity);

        //Collider[] projectileColliders = projetctile.GetComponentsInChildren<Collider>();

        //if (projectileColliders.Length > 0) //i think not working
        //{
        //    for (int i = 0; i < projectileColliders.Length; i++)
        //    {
        //        foreach (Collider playerCollider in playerColliders)
        //        {
        //            Physics.IgnoreCollision(playerCollider, projectileColliders[i]); // Ignore collision between the player and the projectile
        //        }
        //    }
        //}

        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(ownerPlayableState);
        }

        if (projetctile.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection); //Call interface
        }

        //only owner can activate the item
        if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        {
            //Get the ref to active the item
            ItemActivableManager.Instance.SetItemThrowableActivableClient(activable);
        }


        if (IsOwner)
        {
            //only owner can follow the item
            if (projetctile.transform.TryGetComponent(out IFollowable followable))
            {
                followable.Follow(transform); //Call interface
            }

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
