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
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private Player player;

    private IActivable itemActivable;
    private bool itemActivated = false;

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
        Debug.Log($"Debug Touch itemActivated: {itemActivated} ItemActivable: {itemActivable}");

        if(context.started && !itemActivated && itemActivable != null)
        {
            //touched, isnt activated and has an activable item
            itemActivated = true;
            itemActivable.Activate();

            itemActivable = null;

            Debug.Log("Trying to active the item");
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
        itemActivated = false;

        SpawnProjectileServerRpc(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag, player.PlayerInventory.GetSelectedItemSOIndex()); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag, player.PlayerInventory.GetSelectedItemSOIndex()); //Spawn fake projectile on client
    
        OnItemLaunched?.Invoke(player.PlayerInventory.SelectedItemInventoryIndex); //pass itemInventoryIndex
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(float dragForce, Vector3 dragDirection, int selectedItemSOIndex) // on server, need to pass the prefab for the other clients instantiate it
    {
        if(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemServerPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {selectedItemSOIndex} has no server prefab");
            return;
        }


        GameObject gameObject = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemServerPrefab, spawnItemPos.position, Quaternion.identity);

        if (gameObject.TryGetComponent(out Collider projectileCollider))
        {
            foreach(Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(playerCollider, projectileCollider); // Ignore collision between the player and the projectile
            }
        }

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection, transform); //Call interface
        }
        
        SpawnProjectileClientRpc(dragForce, dragDirection, selectedItemSOIndex);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientRpc(float dragForce, Vector3 dragDirection, int selectedItemSOIndex) //pass info to other clients
    {
        if (IsOwner) return; // already spawned

        SpawnDummyProjectile(dragForce, dragDirection, selectedItemSOIndex);

    }

    private void SpawnDummyProjectile(float dragForce, Vector3 dragDirection, int selectedItemSOIndex) // on client, need to pass the prefab for the other clients instantiate it
    {
        if (player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemClientPrefab == null)
        {
            Debug.LogWarning($"ItemSOIndex: {selectedItemSOIndex} has no client prefab");
            return;
        }


        GameObject projetctile = Instantiate(player.PlayerInventory.GetItemSOByItemSOIndex(selectedItemSOIndex).itemClientPrefab, spawnItemPos.position, Quaternion.identity);

        if (projetctile.TryGetComponent(out Collider projectileCollider))
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(playerCollider, projectileCollider); // Ignore collision between the player and the projectile
            }
        }

        if (projetctile.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection, transform); //Call interface
        }

        if (gameObject.transform.TryGetComponent(out IActivable activable))
        {
            //Get the ref to active the item
            itemActivable = activable;
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
