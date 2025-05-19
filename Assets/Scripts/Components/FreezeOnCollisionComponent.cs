using Unity.Netcode;
using UnityEngine;

public class FreezeOnCollisionComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BaseCollisionController baseCollisionController;

    public override void OnGainedOwnership()
    {
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if (!IsOwner) return;
        Debug.Log("Stop moving");
        rb.isKinematic = true; // Freeze the object
    }

    public override void OnLostOwnership()
    {
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }
}
