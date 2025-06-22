using System;
using System.Collections;
using Sortify;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawnerNotNetworked : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemDirection;
    [SerializeField] private ItemSO itemSO;
    
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

    private void Start()
    {
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
                selectedItemID = itemSO.itemID, 
                ownerPlayableState = PlayableState.Player1Playing,
                shootPosition = transform.position,
                shootRotation = transform.rotation,
                isRightSocket = true
            };
            
            Spawn(itemLauncherData);
            yield return delay; // wait to activate if possible
            
        }
    }
    
    private void Spawn(ItemLauncherData launcherData)
    {
        lastProjectile = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(null);
            itemThrowable.ItemReleased(launcherData, true);
        }
    }
}
