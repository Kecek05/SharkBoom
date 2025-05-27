using System;
using UnityEngine;

public class HarpoonItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }

    protected override void OnCollided(GameObject collidedObj)
    {
        rotateTowardsVelocityComponent.DisableComponent();
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        rotateTowardsVelocityComponent.DisableComponent();
    }
}
