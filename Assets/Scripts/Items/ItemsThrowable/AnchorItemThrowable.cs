using Sortify;
using UnityEngine;

public class AnchorItemThrowable : BaseItemThrowableActivable
{
    [BetterHeader("Anchor")]
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;
    [SerializeField] private float downForce;
    [SerializeField] private DamageableSO anchorActivatedDamageableSO;
    [SerializeField] private CanDoDamageComponent canDoDamageComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }
    protected override void ActivateItem()
    {
        
        canDoDamageComponent.SetDamageableSO(anchorActivatedDamageableSO);

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
