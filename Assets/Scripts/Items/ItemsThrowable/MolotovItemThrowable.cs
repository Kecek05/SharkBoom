using System;
using System.Collections;
using Sortify;
using UnityEngine;

public class MolotovItemThrowable : BaseItemThrowableActivable
{
    [BetterHeader("Molotov Item Settings")]
    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private Collider explosionCollider;
    [SerializeField] private AutoActivateItemComponent autoActivateItemComponent;
    private Coroutine explodeMolotovCoroutine;
    private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.5f);
    private WaitForSecondsRealtime waitToDestroy = new WaitForSecondsRealtime(3.5f);

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void ActivateItem()
    {
        canDoDamageComponent.SetDamageableSO(damageableSOActivated);
        
        explodeMolotovCoroutine ??= StartCoroutine(ExplodeMolotov());
    }

    protected override void CollisionController_OnCollidedWithoutPlayer(GameObject collidedObject)
    {
        if(!itemActivated) //Not exploded yet and didnt collided with player
            autoActivateItemComponent.SelfActivate();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        spinObjectComponent.DisableComponent();
    }

    private IEnumerator ExplodeMolotov()
    {
        lifetimeTriggerItemComponent.StopLifetime(); //prevent the item to be destroyed while it's exploding

        rb.isKinematic = true; // Stop bomb
        explosionCollider.enabled = true;
        spinObjectComponent.DisableComponent();

        yield return waitForSecondsRealtime;

        explosionCollider.enabled = false;

        explodeMolotovCoroutine = null;

        yield return waitToDestroy;

        base.DestroyItem();
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
    }
}
