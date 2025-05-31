using System;
using System.Collections;
using UnityEngine;

public class BombItemThrowable : BaseItemThrowableActivable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private Collider explosionCollider;
    private Coroutine explodeBombCoroutine;
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);
    private WaitForSecondsRealtime waitToDestroy = new WaitForSecondsRealtime(2f);

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void ActivateItem()
    {
        itemActivated = true;

        explodeBombCoroutine ??= StartCoroutine(ExplodeBomb());
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        base.CollisionController_OnCollided(collidedObject);

        spinObjectComponent.DisableComponent();
    }

    private IEnumerator ExplodeBomb()
    {
        lifetimeTriggerItemComponent.StopLifetime(); //prevent the item to be destroyed while is exploding

        rb.isKinematic = true; // Stop bomb
        explosionCollider.enabled = true;
        spinObjectComponent.DisableComponent();

        yield return waitForSecondsRealtime;

        explosionCollider.enabled = false;

        explodeBombCoroutine = null;

        yield return waitToDestroy;

        DestroyItem();
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
    }
}
