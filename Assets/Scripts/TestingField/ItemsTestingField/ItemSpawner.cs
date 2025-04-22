using Sortify;
using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [SerializeField] private ItemSO itemToSpawnSO;
    [SerializeField] private Transform itemDirection;

    [BetterHeader("Item Spawner Settings", 10)]
    [SerializeField] private float dragForce;
    [SerializeField] private float delayBetweenSpawns = 3f;
    [SerializeField] private bool canSpawn = true;

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
                dragForce = dragForce,
                dragDirection = itemDirection.position,
                selectedItemSOIndex = 0, // irrelevant
                ownerPlayableState = PlayableState.None, // irrelevant
            };


            SpawnItemProjectile(itemLauncherData);

            yield return delay; // wait to activate if possible

            BaseItemThrowableActivable lastActivableProjectile = lastProjectile.GetComponent<BaseItemThrowableActivable>();

            if(lastActivableProjectile != null)
            {

                lastActivableProjectile.TryActivate();
            }
        }
    }


    private void SpawnItemProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {

        lastProjectile = Instantiate(itemToSpawnSO.itemClientPrefab, transform.position, Quaternion.identity);

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(null);
            itemThrowable.ItemReleased(launcherData);
        }

        //if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        //{
        //    //Get the ref to active the item
        //    itemActivableManager.SetItemThrowableActivableClient(activable);
        //}

    }
}
