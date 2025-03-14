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
        if (newState != player.PlayerStateMachine.draggingItem && newState != player.PlayerStateMachine.draggingJump)
        {
            DoOnDragStopped();
        }
    }

    protected abstract void DoOnSpawn();

    protected abstract void DoOnDragChange();

    protected abstract void DoOnDragStopped();


    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
            player.PlayerDragController.OnDragChange -= PlayerDragController_OnDragChange;
        }
    }
}
