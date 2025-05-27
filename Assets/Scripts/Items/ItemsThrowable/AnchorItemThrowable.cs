using System;
using UnityEngine;

public class AnchorItemThrowable : BaseItemThrowableActivable
{
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;
    [SerializeField] private float downForce;
    [SerializeField] private DamageableSO anchorActivatedDamageableSO;
    [SerializeField] private CanDoDamageComponent canDoDamageComponent;

    protected override void CollisionController_OnCollided(GameObject collidedObject)
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }
    protected override void ActivateItem()
    {
        itemActivated = true;
        
        canDoDamageComponent.SetDamageableSO(anchorActivatedDamageableSO);

        rb.linearVelocity = Vector3.zero; // Stop the item from moving
        rb.AddForce(Vector3.down * downForce, ForceMode.Impulse);
    }

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        rotateTowardsVelocityComponent.DisableComponent();
    }
}
