using System;
using UnityEngine;

public class SeaStarItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    protected override void OnCollided(GameObject collidedObj)
    {
        spinObjectComponent.DisableComponent();
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        spinObjectComponent.DisableComponent();
    }

}
