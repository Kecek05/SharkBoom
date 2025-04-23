using Sortify;
using UnityEngine;

public class PlayerRotateToAim : DragListener, IInitializeOnwer, IDetectDragChange, IDetectEndedTurn
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private PlayerDragController playerDragController;

    protected override void DoOnInitializeOnwer()
    {

    }

    protected override void DoOnDragChange(float forcePercent, float angle)
    {

    }

    protected override void DoOnDragRelease()
    {

    }

    protected override void DoOnEndedTurn()
    {
        
    }

    void IInitializeOnwer.DoOnInitializeOnwer()
    {
        aimTransform.position = aimDefaultPosition.position;
    }

    void IDetectDragChange.DoOnDragChange(float forcePercent, float andlePercent)
    {
        aimTransform.position = playerDragController.GetOpositeFingerPos();
    }

    void IDetectEndedTurn.DoOnEndedTurn()
    {
        aimTransform.position = aimDefaultPosition.position;
    }
}
