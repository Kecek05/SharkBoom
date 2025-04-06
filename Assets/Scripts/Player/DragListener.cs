using Unity.Netcode;

public abstract class DragListener : NetworkBehaviour
{

    public void InitializeOwner()
    {
        if (!IsOwner) return;
        DoOnSpawn();
    }

    public void HandleOnPlayerDragControllerDragChange(float forcePercent, float angle)
    {
        if(!IsOwner) return; //only owner
        DoOnDragChange(forcePercent, angle);
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if (!IsOwner) return; //only owner
        if (newState == PlayerState.DragReleaseItem && newState == PlayerState.DraggingJump)
        {
            DoOnDragRelease();
        }

        if(newState == PlayerState.IdleMyTurn || newState == PlayerState.IdleEnemyTurn) // for dont show the infos of drag when we are not dragging
        {
            DoOnDragRelease();
        }
    }

    /// <summary>
    /// Called when the object is spawned and its the owner
    /// </summary>
    protected abstract void DoOnSpawn();

    /// <summary>
    /// Called when the finger changes position
    /// </summary>
    protected abstract void DoOnDragChange(float forcePercent, float andlePercent);

    /// <summary>
    /// Called when the drag is released, the object is launched.
    /// </summary>
    protected abstract void DoOnDragRelease();

}
