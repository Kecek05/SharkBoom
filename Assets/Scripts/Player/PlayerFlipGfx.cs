using Sortify;
using Unity.Netcode;
using UnityEngine;

public class PlayerFlipGfx : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private Transform playerGfxTransform;
    private Vector3 startEulerAngles;

    protected override void DoOnSpawn()
    {
        startEulerAngles = playerGfxTransform.eulerAngles;
    }

    protected override void DoOnDragChange()
    {
        if(player.PlayerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, -90f , transform.eulerAngles.z);
        else if (player.PlayerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 90f, transform.eulerAngles.z);
    }

    protected override void DoOnDragStopped()
    {
        playerGfxTransform.eulerAngles = startEulerAngles;
    }

}
