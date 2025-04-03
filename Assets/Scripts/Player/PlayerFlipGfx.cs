using Sortify;
using UnityEngine;

public class PlayerFlipGfx : DragListener
{
    [BetterHeader("References")]
    [SerializeField] private Transform playerGfxTransform;
    [SerializeField] private PlayerDragController playerDragController;

    [Tooltip("Value to be add to not rotate the object to close to the 90 degrees")]
    [SerializeField] private float angleOffset = 0.5f;
    private Vector3 startEulerAngles;

    protected override void DoOnSpawn()
    {
        startEulerAngles = playerGfxTransform.eulerAngles;
    }

    protected override void DoOnDragChange()
    {
        if (playerDragController.GetOpositeFingerPos().x > playerGfxTransform.position.x + angleOffset)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, -90f , transform.eulerAngles.z);
        else if (playerDragController.GetOpositeFingerPos().x < playerGfxTransform.position.x - angleOffset)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 90f, transform.eulerAngles.z);
    }

    protected override void DoOnDragRelease()
    {
        playerGfxTransform.eulerAngles = startEulerAngles;
    }

}
