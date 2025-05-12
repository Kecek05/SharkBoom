using Sortify;
using UnityEngine;

public class PlayerRotateToAim : DragListener, IInitializeOnwer, IDetectDragChange, IDetectEndedTurn
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private PlayerDragController playerDragController;

    public void DoOnInitializeOnwer()
    {
        aimTransform.position = aimDefaultPosition.position;
    }

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        aimTransform.position = playerDragController.GetOpositeFingerPos();
    }

    public void DoOnEndedTurn()
    {
        aimTransform.position = aimDefaultPosition.position;
    }
}
