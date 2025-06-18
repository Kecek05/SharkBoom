using System;
using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        spinObjectComponent.DisableComponent();
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
    }
}
