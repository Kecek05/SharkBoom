using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerRotateToAim : NetworkBehaviour
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private Player player;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
            player.PlayerDragController.OnDragChange += PlayerDragController_OnDragChange;
        }
    }

    private void PlayerDragController_OnDragChange()
    {
        //Change the pos of aim when the finger change pos
        aimTransform.position = player.PlayerDragController.GetOpositeFingerPos();
    }

    private void PlayerStateMachine_OnStateChanged(IState newState)
    {
        if(newState != player.PlayerStateMachine.draggingItem && newState != player.PlayerStateMachine.draggingJump)
        {
            // stop changing pos of aim
            aimTransform.position = aimDefaultPosition.position;
        }
    }



    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
            player.PlayerDragController.OnDragChange -= PlayerDragController_OnDragChange;
        }
    }
}
