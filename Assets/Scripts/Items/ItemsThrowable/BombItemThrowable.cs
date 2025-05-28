using System;
using System.Collections;
using UnityEngine;

public class BombItemThrowable : BaseItemThrowableActivable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private Collider explosionCollider;
    private Coroutine explodeBombCoroutine;
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);

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


    private IEnumerator ExplodeBomb()
    {

        rb.isKinematic = true; // Stop bomb
        explosionCollider.enabled = true;
        spinObjectComponent.DisableComponent();

        yield return waitForSecondsRealtime;

        explosionCollider.enabled = false;

        explodeBombCoroutine = null;
    }

    protected override void CollisionController_OnCollided(GameObject collidedObj)
    {
        TryActivate(); //Explode event if touch ground
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        spinObjectComponent.DisableComponent();
    }
}
