using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [SerializeField] private ItemSO itemToSpawnSO;

    [SerializeField] private float dragForce;
    [SerializeField] private Transform harpoonDirection;
    [SerializeField] private float delayBetweenSpawns = 3f;

    private GameObject lastProjectile;

    private void OnDrawGizmos()
    {
        float lineLength = 3f;


        Gizmos.color = Color.blue;
        Vector3 start = transform.position;

        Vector3 end = harpoonDirection.position;

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
            WaitForSeconds delay = new WaitForSeconds(delayBetweenSpawns);

            WaitForSeconds delayToActivate = new WaitForSeconds(delayBetweenSpawns / 2f);

            yield return delay;

            ItemLauncherData itemLauncherData = new ItemLauncherData
            {
                dragForce = dragForce,
                dragDirection = harpoonDirection.position,
                selectedItemSOIndex = 0, // irrelevant
                ownerPlayableState = PlayableState.None, // irrelevant
            };


            SpawnItemProjectile(itemLauncherData); 

            BaseItemThrowableActivable lastActivableProjectile = lastProjectile.GetComponent<BaseItemThrowableActivable>();

            if(lastActivableProjectile != null)
            {
                yield return delayToActivate; // wait to activate

                lastActivableProjectile.TryActivate();
            }
        }
    }


    private void SpawnItemProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {

        lastProjectile = Instantiate(itemToSpawnSO.itemClientPrefab, transform.position, Quaternion.identity);


        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.Initialize(launcherData);
        }

        //if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        //{
        //    //Get the ref to active the item
        //    itemActivableManager.SetItemThrowableActivableClient(activable);
        //}

    }
}
