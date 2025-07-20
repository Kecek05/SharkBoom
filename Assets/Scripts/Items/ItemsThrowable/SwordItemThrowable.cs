using System;
using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private BaseItemComponent flipObjectComponent;
    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        spinObjectComponent.EnableComponent();
        flipObjectComponent.EnableComponent();
        
        flipObjectComponent.StartComponentLogic();
        spinObjectComponent.StartComponentLogic();
    }

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        // flipObjectComponent.DisableComponent();
        spinObjectComponent.DisableComponent();
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        spinObjectComponent.DisableComponent();
        flipObjectComponent.DisableComponent();
    }
}
