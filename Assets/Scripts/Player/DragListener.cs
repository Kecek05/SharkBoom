using Sortify;
using Unity.Netcode;
using UnityEngine;

public abstract class DragListener : NetworkBehaviour
{
    [BetterHeader("DragListener References")]
    [SerializeField] protected Player player;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
            player.PlayerDragController.OnDragChange += PlayerDragController_OnDragChange;

            DoOnSpawn();
        }
    }

    private void PlayerDragController_OnDragChange()
    {
        DoOnDragChange();
    }

    private void PlayerStateMachine_OnStateChanged(IState newState)
    {
        if (newState == player.PlayerStateMachine.dragReleaseItem && newState == player.PlayerStateMachine.dragReleaseJump)
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
    protected abstract void DoOnDragChange();

    /// <summary>
    /// Called when the drag is released, the object is launched.
    /// </summary>
    protected abstract void DoOnDragRelease();


    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
            player.PlayerDragController.OnDragChange -= PlayerDragController_OnDragChange;
        }
    }
}
