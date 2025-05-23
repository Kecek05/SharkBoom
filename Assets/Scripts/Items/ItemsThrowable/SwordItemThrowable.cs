using System;
using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;
    [SerializeField] private BaseCollisionController collisionController;

    public override void Initialize(Transform parent)
    {
        base.Initialize(parent);

        collisionController.OnCollided += OnCollided; //Subscribe to the collision event
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    private void OnCollided(GameObject collidedObj)
    {
        spinObjectComponent.DisableComponent();
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        collisionController.OnCollided -= OnCollided; //Subscribe to the collision event

        spinObjectComponent.DisableComponent();
    }
}
