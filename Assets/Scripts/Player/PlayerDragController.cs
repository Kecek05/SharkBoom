
public class PlayerDragController : DragAndShoot
{

    public void HandleOnPlayerStateMachineStateChanged(PlayerState state)
    {
        if(state == PlayerState.IdleMyTurn)
        {
            //First Reset it
            TurnOffDrag();
            ResetDrag();
            
            //Then turn it on
            TurnOnDrag();
        } 
        else if (state == PlayerState.DragReleaseJump || state == PlayerState.DragReleaseItem || state == PlayerState.IdleEnemyTurn || state == PlayerState.MyTurnEnded)
        {
            TurnOffDrag();
            ResetDrag();
        } else if (state == PlayerState.PlayerGameOver)
        {
            TurnOffDrag();
        }
    }

}
