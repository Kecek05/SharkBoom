using Sortify;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerRotateToAim : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform aimDefaultPosition;

    protected override void DoOnSpawn()
    {
        aimTransform.position = aimDefaultPosition.position;
    }

    protected override void DoOnDragChange()
    {
        aimTransform.position = player.PlayerDragController.GetOpositeFingerPos();
    }

    protected override void DoOnDragStopped()
    {
        aimTransform.position = aimDefaultPosition.position;
    }

}
