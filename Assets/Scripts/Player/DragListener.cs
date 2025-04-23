using Unity.Netcode;
using UnityEngine;

public abstract class DragListener : NetworkBehaviour
{

    public void InitializeOwner()
    {
        if (!IsOwner) return;
        DoOnInitializeOnwer();
    }

    public void HandleOnPlayerDragControllerDragChange(float forcePercent, float angle)
    {
        if(!IsOwner) return; //only owner
        DoOnDragChange(forcePercent, angle);
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if (!IsOwner) return; //only owner

        if (newState == PlayerState.DragReleaseItem || newState == PlayerState.DragReleaseJump)
        {
            Debug.Log("Drag Release Item");
            DoOnDragRelease();
        } else if (newState == PlayerState.MyTurnEnded)
        {
            DoOnEndedTurn();
        }
    }

    /// <summary>
    /// Called when the object is spawned and its the owner
    /// </summary>
    protected abstract void DoOnInitializeOnwer();

    /// <summary>
    /// Called when the finger changes position
    /// </summary>
    protected abstract void DoOnDragChange(float forcePercent, float andlePercent);

    /// <summary>
    /// Called when the drag is released, the object is launched.
    /// </summary>
    protected abstract void DoOnDragRelease();

    protected abstract void DoOnEndedTurn();
}
