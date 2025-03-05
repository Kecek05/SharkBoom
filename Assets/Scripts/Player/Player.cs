using Sortify;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    [BetterHeader("References")]
    [SerializeField] private Transform spawnThrowablePos;
    [SerializeField] private PlayerInventory playerInventory;
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

        SpawnProjectileServerRpc(dragAndShoot.DragForce, dragAndShoot.DirectionOfDrag); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(dragAndShoot.DragForce, dragAndShoot.DirectionOfDrag); //Spawn fake projectile on client

        dragAndShoot.ResetDragPos();
    }

    [Rpc(SendTo.Server)]
    private void SpawnProjectileServerRpc(float dragForce, Vector3 dragDirection) // on server 
    {
        GameObject gameObject = Instantiate(serverProjectileDebug, spawnThrowablePos.position, Quaternion.identity);

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
        if(IsOwner) return; // already spawned

        SpawnDummyProjectile(dragForce, dragDirection); 

        Debug.Log("Spawn Projectile Client");
    }

    private void SpawnDummyProjectile(float dragForce, Vector3 dragDirection)
    {

        GameObject gameObject = Instantiate(clientProjectilePrefabDebug, spawnThrowablePos.position, Quaternion.identity);

        Physics.IgnoreCollision(playerCollider, gameObject.GetComponent<Collider>()); // Ignore collision between the player and the projectile

        if (gameObject.transform.TryGetComponent(out IDraggable draggable))
        {
            draggable.Release(dragForce, dragDirection); //Call interface
        }
    }


    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;

        dragAndShoot.OnDragRelease -= DragAndShoot_OnDragRelease;
    }

    public ItemSO GetSelectedItemSO() // used for get the mass, // used for get the mass, probably we will refactor for get all the itemSO, but for now 
    {

        ItemSO selectedItemSO = playerInventory.GetItemSOByIndex(playerInventory.SelectedItemData.itemSOIndex); // get the itemSO from the player inventory and the index

        if (selectedItemSO == null) // check if the itemSO is null for not break the code
        {
            Debug.LogWarning("Didnt found the ItemSO!");
            return null;
        }

        return selectedItemSO; // get the mass of the itemSO
    }
}
