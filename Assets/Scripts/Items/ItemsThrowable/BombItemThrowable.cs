using System;
using System.Collections;
using UnityEngine;

public class BombItemThrowable : BaseItemThrowableActivable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private Collider explosionCollider;
    private Coroutine explodeBombCoroutine;
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);
    private WaitForSecondsRealtime waitToDestroy = new WaitForSecondsRealtime(3.5f);

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void ActivateItem()
    {
        explodeBombCoroutine ??= StartCoroutine(ExplodeBomb());
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        spinObjectComponent.DisableComponent();
    }

    private IEnumerator ExplodeBomb()
    {
        lifetimeTriggerItemComponent.StopLifetime(); //prevent the item to be destroyed while it's exploding

        rb.isKinematic = true; // Stop bomb
        explosionCollider.enabled = true;
        spinObjectComponent.DisableComponent();

        yield return waitForSecondsRealtime;

        explosionCollider.enabled = false;

        explodeBombCoroutine = null;

        yield return waitToDestroy;

        base.DestroyItem();
    }
    
    public override void DestroyItem(Action destroyedCallback = null)
    {   
        //if (!IsOwner) return;

        if (!itemReleased)
        {
            base.DestroyItem(destroyedCallback);
            return;
        }
        
        //If the lifetime of the bomb gets to the end, it will explode
        if(itemCanBeActivated && !itemActivated) //Not exploded yet
            TryActivate();
        else
            base.DestroyItem(destroyedCallback);
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
    }
}
