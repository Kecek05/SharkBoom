using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerLauncher : NetworkBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private Transform spawnItemPos;
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private Player player;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerDragController.OnDragRelease += PlayerDragAndShoot_OnDragRelease;
        }
    }

    [Command("playerLauncher-release")]
    private void PlayerDragAndShoot_OnDragRelease()
    {
        //Spawn Object, only owner
        SpawnProjectileServerRpc(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(player.PlayerDragController.DragForce, player.PlayerDragController.DirectionOfDrag); //Spawn fake projectile on client

        //CameraManager.Instance.SetCameraState(CameraManager.CameraState.Following);
        //CameraManager.Instance.CameraFollowing.SetCameraFollowingObject(projetctile.transform);
        //if (player.PlayerInventory.SelectedItemData.Value.itemInventoryIndex == 0) //Jump
        //{
        //    player.PlayerJumped();

        //} else
        //{
        //    player.PlayerShooted();
        //}

        if (player.PlayerInventory.SelectedItemIndex.Value == 0) //Jump
        {
            player.PlayerJumped();

        }
        else
        {
            player.PlayerShooted();
        }
        Debug.Log($"Release, SelectedItemIndex: {player.PlayerInventory.SelectedItemIndex.Value}");
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(float dragForce, Vector3 dragDirection) // on server 
    {
        if (player.PlayerInventory.GetSelectedItemSO().itemServerPrefab == null) return;

        GameObject gameObject = Instantiate(player.PlayerInventory.GetSelectedItemSO().itemServerPrefab, spawnItemPos.position, Quaternion.identity);

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
        
        SpawnProjectileClientRpc(dragForce, dragDirection);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientRpc(float dragForce, Vector3 dragDirection) // on client
    {
        if (IsOwner) return; // already spawned

        SpawnDummyProjectile(dragForce, dragDirection);

    }

    private void SpawnDummyProjectile(float dragForce, Vector3 dragDirection)
    {

        if (player.PlayerInventory.GetSelectedItemSO().itemClientPrefab == null) return;


        GameObject projetctile = Instantiate(player.PlayerInventory.GetSelectedItemSO().itemClientPrefab, spawnItemPos.position, Quaternion.identity);

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

    }

    public override void OnNetworkDespawn()
    {
        if(IsOwner)
        {
            player.PlayerDragController.OnDragRelease -= PlayerDragAndShoot_OnDragRelease;
        }
    }

}
