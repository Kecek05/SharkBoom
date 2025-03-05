using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public event Action OnPlayerReady;

    [BetterHeader("References")]
    [SerializeField] private Transform spawnThrowablePos;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerInventoryUI playerInventoryUI;
    [SerializeField] private DragAndShoot dragAndShoot;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private GameObject clientProjectilePrefabDebug;
    [SerializeField] private GameObject serverProjectileDebug;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            dragAndShoot.Initialize();

            GameFlowManager.OnRoundStarted += GameFlowManager_OnRoundStarted;
        }
    }

    public void SetPlayerReady() //UI Button will call this
    {

        GameFlowManager.Instance.SetPlayerReadyServerRpc();

        OnPlayerReady?.Invoke();

        Debug.Log("Player Setted to Ready");
    }

    private void GameFlowManager_OnRoundStarted()
    {
        //Spawn Object
        SpawnProjectileServerRpc(dragAndShoot.DragForce, dragAndShoot.DirectionOfDrag); //Spawn real projectile on server need to send the speed and force values through the network

        SpawnDummyProjectile(dragAndShoot.DragForce, dragAndShoot.DirectionOfDrag); //Spawn fake projectile on client
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
        if (IsOwner)
        {

            GameFlowManager.OnRoundStarted -= GameFlowManager_OnRoundStarted;
        }
    }

    public ItemSO GetSelectedItemSO()
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
