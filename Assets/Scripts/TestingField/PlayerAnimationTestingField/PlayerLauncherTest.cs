using UnityEngine;

public class PlayerLauncherTest : MonoBehaviour
{

    [SerializeField] private Transform itemDirection;
    [SerializeField] private float dragForce;
    private BaseItemThrowable lastProjectile;

    public void HandleOnItemOnHandSpawned(BaseItemThrowable baseItemThrowable)
    {
        lastProjectile = baseItemThrowable;
    }

    public void HandleOnItemOnHandDespawned(BaseItemThrowable baseItemThrowable)
    {
        if (baseItemThrowable == lastProjectile)
        {
            lastProjectile = null;
        }
    }

    public void Launch()
    {
        ItemLauncherData itemLauncherData = new ItemLauncherData
        {
            dragForce = dragForce,
            dragDirection = itemDirection.position,
            selectedItemSOIndex = 0, // irrelevant
            ownerPlayableState = PlayableState.None, // irrelevant
        };

        if (lastProjectile.transform.TryGetComponent(out BaseItemThrowable itemThrowable))
        {
            itemThrowable.ItemReleased(itemLauncherData);
        }

        //if (projetctile.transform.TryGetComponent(out BaseItemThrowableActivable activable))
        //{
        //    //Get the ref to active the item
        //    itemActivableManager.SetItemThrowableActivableClient(activable);
        //}

    }
}
