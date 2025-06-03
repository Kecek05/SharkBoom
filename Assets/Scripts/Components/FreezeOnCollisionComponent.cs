using Unity.Netcode;
using UnityEngine;

public class FreezeOnCollisionComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();
        UnfreezeObjectServerRpc();
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if(!IsOwner) return;
        FreezeObjectServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void FreezeObjectServerRpc()
    {
        FreezeObjectClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FreezeObjectClientRpc()
    {
        rb.isKinematic = true; // Freeze the object
    }

    [Rpc(SendTo.Server)]
    private void UnfreezeObjectServerRpc()
    {
        UnfreezeObjectClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UnfreezeObjectClientRpc()
    {
        rb.isKinematic = false; // Unfreeze the object
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }

}
