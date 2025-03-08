using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerDragController : DragAndShoot
{
    [BetterHeader("References")]
    [SerializeField] private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;

    }

    private void PlayerStateMachine_OnStateChanged(IState state)
    {
        if(state == player.PlayerStateMachine.idleMyTurnState)
        {
            TurnOnDrag();
        } 
        else if (state == player.PlayerStateMachine.dragReleaseJump || state == player.PlayerStateMachine.dragReleaseItem)
        {
            TurnOffDrag();
            ResetDrag();
        }
    }


    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
    }
}
