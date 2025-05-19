using System;
using UnityEngine;

public class HarpoonItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;
    [SerializeField] private BaseCollisionController collisionController;

    public override void Initialize(Transform parent)
    {
        base.Initialize(parent);

        collisionController.OnCollided += OnCollided; //Subscribe to the collision event
    }



    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }

    private void OnCollided(GameObject collidedObj)
    {
        rotateTowardsVelocityComponent.DisableComponent();

        rb.freezeRotation = true; //freeze rotation to avoid the spear to rotate when it hits something DEBUG
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        collisionController.OnCollided -= OnCollided; //Subscribe to the collision event
    }
}
