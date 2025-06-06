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
        ResetAimPosition();
    }

    public void DoOnDragChange(float forcePercent, float andlePercent)
    {
        aimTransform.position = playerDragController.GetOpositeFingerPos();
    }
    
    private void ResetAimPosition()
    {
        aimTransform.position = aimDefaultPosition.position;
    }
    
    public void DoOnGetUp()
    {
        ResetAimPosition();
    }

    public void DoOnEndedTurn()
    {
        ResetAimPosition();
    }
}
