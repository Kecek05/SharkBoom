using UnityEngine;

public class HarpoonItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent rotateTowardsVelocityComponent;

    public override void Initialize(PlayableState ownerPlayableState)
    {
        base.Initialize(ownerPlayableState);

        rotateTowardsVelocityComponent.EnableComponent();

        rotateTowardsVelocityComponent.StartComponentLogic();
    }


    //DEBUG

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rotateTowardsVelocityComponent.DisableComponent();

        rb.freezeRotation = true; //freeze rotation to avoid the spear to rotate when it hits something DEBUG
    }
}
