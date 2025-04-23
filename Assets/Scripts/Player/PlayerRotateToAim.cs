using Sortify;
using UnityEngine;

public class PlayerRotateToAim : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private PlayerDragController playerDragController;

    protected override void DoOnInitializeOnwer()
    {
        aimTransform.position = aimDefaultPosition.position;
    }

    protected override void DoOnDragChange(float forcePercent, float angle)
    {
        aimTransform.position = playerDragController.GetOpositeFingerPos();
    }

    protected override void DoOnDragRelease()
    {

    }

    protected override void DoOnEndedTurn()
    {
        aimTransform.position = aimDefaultPosition.position;
    }
}
