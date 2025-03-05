using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerLauncher : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Collider playerCollider;
    [SerializeField] private Transform spawnItemPos;
    [SerializeField] private Player player;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerDragAndShoot.Initialize();

            GameFlowManager.OnRoundStarted += GameFlowManager_OnRoundStarted;
        }
    }

    private void GameFlowManager_OnRoundStarted()
    {
        //Spawn Object, only owner
        SpawnProjectileServerRpc(player.PlayerDragAndShoot.DragForce, player.PlayerDragAndShoot.DirectionOfDrag); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(player.PlayerDragAndShoot.DragForce, player.PlayerDragAndShoot.DirectionOfDrag); //Spawn fake projectile on client
    }



    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(float dragForce, Vector3 dragDirection) // on server 
    {
        GameObject gameObject = Instantiate(player.PlayerInventory.GetSelectedItemSO().itemServerPrefab, spawnItemPos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection); //Call interface
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

        GameObject gameObject = Instantiate(player.PlayerInventory.GetSelectedItemSO().itemClientPrefab, spawnItemPos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection); //Call interface

        } else if (gameObject.transform.TryGetComponent(out IUseable useable))
        {
            useable.Use(); //Call interface
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsOwner)
        {
            GameFlowManager.OnRoundStarted -= GameFlowManager_OnRoundStarted;
        }
    }

}
