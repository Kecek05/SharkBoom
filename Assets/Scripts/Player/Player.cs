using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private Transform spawnThrowablePos;
    [SerializeField] private DragAndShoot dragAndShoot;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private GameObject clientProjectilePrefabDebug;
    [SerializeField] private GameObject serverProjectileDebug;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            dragAndShoot.Initialize();

            dragAndShoot.OnDragRelease += DragAndShoot_OnDragRelease;
        }
    }

    private void DragAndShoot_OnDragRelease()
    {
        //REFACTOR

        SpawnProjectileServerRpc(); //Spawn real projectile on server

        SpawnDummyProjectile(); //Spawn fake projectile on client

        dragAndShoot.ResetDragPos();
    }

    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc() // on server 
    {
        GameObject gameObject = Instantiate(serverProjectileDebug, spawnThrowablePos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragAndShoot.Force, dragAndShoot.Direction); //Call interface
        }

        SpawnProjectileClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnProjectileClientRpc() // on client
    {
        if(IsOwner) return; // already spawned

        SpawnDummyProjectile();
    }

    private void SpawnDummyProjectile()
    {

        GameObject gameObject = Instantiate(clientProjectilePrefabDebug, spawnThrowablePos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragAndShoot.Force, dragAndShoot.Direction); //Call interface
        }
    }


    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;

        dragAndShoot.OnDragRelease -= DragAndShoot_OnDragRelease;
    }

}
