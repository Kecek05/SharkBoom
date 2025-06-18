using System;
using UnityEngine;

public class SeaStarItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObj)
    {
        spinObjectComponent.DisableComponent();
        rb.useGravity = true;
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
        rb.useGravity = false;
    }

}
