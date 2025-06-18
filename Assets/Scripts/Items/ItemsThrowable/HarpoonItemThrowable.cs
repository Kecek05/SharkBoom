using System;
using UnityEngine;

public class HarpoonItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObj)
    {
        rotateTowardsVelocityComponent.DisableComponent();
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        rotateTowardsVelocityComponent.DisableComponent();
    }
}
