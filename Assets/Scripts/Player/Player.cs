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
    [SerializeField] private GameObject projectilePrefabDebug;

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
        GameObject gameObject = Instantiate(projectilePrefabDebug, spawnThrowablePos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragAndShoot.Force, dragAndShoot.Direction); //Call interface
        }
    }



    public override void OnNetworkDespawn()
    {
        dragAndShoot.OnDragRelease -= DragAndShoot_OnDragRelease;
    }

}
