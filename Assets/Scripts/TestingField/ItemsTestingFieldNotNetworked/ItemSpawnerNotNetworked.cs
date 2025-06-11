using System.Collections;
using Sortify;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawnerNotNetworked : NetworkBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemDirection;
    
    [BetterHeader("Item Spawner Settings", 10)]
    [SerializeField] private float dragForce;
    [SerializeField] private float delayBetweenSpawns = 3f;
    [SerializeField] private bool canSpawn = true;
    [SerializeField] private bool canActivate = true;
    [SerializeField] private bool sync = true;
    private GameObject lastProjectile;

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Vector3 start = transform.position;

        Vector3 end = itemDirection.position;

        Gizmos.DrawLine(start, end);
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        // Start the coroutine to spawn the item
        StartCoroutine(SpawnItem());
    }

    private IEnumerator SpawnItem()
    {

        while (true)
        {

            //In while to be possible to change the delays in runtime
            WaitForSeconds delay = new WaitForSeconds(delayBetweenSpawns / 2f);

            yield return delay;

            if (!canSpawn) continue;

            ItemLauncherData itemLauncherData = new ItemLauncherData
            {
                dragForce = this.dragForce,
                dragDirection = itemDirection.position,
                selectedItemSOIndex = 0, // irrelevant
                ownerPlayableState = PlayableState.None, // irrelevant
            };

            if(sync)
                SpawnItemProjectileServerRpc(itemLauncherData);
            else 
                Spawn(itemLauncherData);
            yield return delay; // wait to activate if possible
            
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnItemProjectileServerRpc(ItemLauncherData launcherData)
    {
        
        SpawnItemProjectileClientRpc(launcherData);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnItemProjectileClientRpc(ItemLauncherData launcherData)
    {
        Spawn(launcherData);
    }
    
    private void Spawn(ItemLauncherData launcherData)
    {
        lastProjectile = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(null);
            itemThrowable.ItemReleased(launcherData);
        }
    }
}
