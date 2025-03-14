using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerRotateToAim : NetworkBehaviour
{
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;
    [SerializeField] private Player player;
    [SerializeField] private Vector3 aimPosOffset;
    [SerializeField] private Transform playerPos;

    private Coroutine changeAimPositionCoroutine;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState newState)
    {
        if(newState == player.PlayerStateMachine.draggingItem || newState == player.PlayerStateMachine.draggingJump)
        {
            //Change pos of aim
            changeAimPositionCoroutine = StartCoroutine(ChangeAimPosition());
        }
        else
        {
            // stop changing pos of aim
            if (changeAimPositionCoroutine != null)
            {
                StopCoroutine(changeAimPositionCoroutine);
            }

            aimTransform.position = aimDefaultPosition.position;
        }
    }

    private IEnumerator ChangeAimPosition()
    {
        while (true)
        {
            aimTransform.position = (playerPos.position - player.PlayerDragController.EndPosDrag) + playerPos.position;
            yield return null;
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
        }
    }
}
