using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [SerializeField] private ItemSO itemToSpawnSO;

    [SerializeField] private float dragForce;
    [SerializeField] private Transform harpoonDirection;

    private void OnDrawGizmos()
    {
        float lineLength = 3f;
        float radius = 0.2f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);


        Gizmos.color = Color.blue;
        Vector3 start = transform.position;

        // End point based on forward direction and line length
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

        while(true)
        {
            yield return new WaitForSeconds(3f);

            ItemLauncherData itemLauncherData = new ItemLauncherData
            {
                dragForce = dragForce,
                dragDirection = harpoonDirection.position,
                selectedItemSOIndex = 0, // irrelevant
                ownerPlayableState = PlayableState.None,
            };


            SpawnItemProjectile(itemLauncherData); 

        }
    }


    private void SpawnItemProjectile(ItemLauncherData launcherData) // on client, need to pass the prefab for the other clients instantiate it
    {

        GameObject projetctile = Instantiate(itemToSpawnSO.itemClientPrefab, transform.position, Quaternion.identity);


        if (projetctile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
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
