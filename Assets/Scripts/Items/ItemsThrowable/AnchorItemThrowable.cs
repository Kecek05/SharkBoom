using System;
using Sortify;
using UnityEngine;

public class AnchorItemThrowable : BaseItemThrowableActivable
{
    [BetterHeader("Anchor")]
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;
    [SerializeField] private float downForce;
    
    
    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }
    protected override void ActivateItem()
    {
        canDoDamageComponent.SetDamageableSO(damageableSOActivated);

        rb.linearVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePositionX;
        rb.AddForce(Vector3.down * downForce, ForceMode.Impulse);
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        rotateTowardsVelocityComponent.DisableComponent();
    }
}
