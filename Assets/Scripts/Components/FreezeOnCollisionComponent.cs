using Unity.Netcode;
using UnityEngine;

public class FreezeOnCollisionComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private BaseCollisionController baseCollisionController;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if (!IsOwner) return;

        rb2D.bodyType = RigidbodyType2D.Static; // Freeze the object
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }
}
