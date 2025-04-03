
public class PlayerDragController : DragAndShoot
{

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {
        if(state == PlayerState.IdleMyTurn)
        {
            TurnOnDrag();
        } 
        else if (state == PlayerState.DragReleaseJump || state == PlayerState.DragReleaseItem || state == PlayerState.IdleEnemyTurn)
        {
            TurnOffDrag();
            ResetDrag();
        } else if (state == PlayerState.PlayerGameOver)
        {
            TurnOffDrag();
        }
    }

}
