using Unity.Netcode;
using UnityEngine;

public class DisableCollisionOnCantactComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
    }

    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();

        EnableCollisionsServerRpc();
    }


    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if(!IsOwner) return;
        DisableCollisionsServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void DisableCollisionsServerRpc()
    {
        DisableCollisionsClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisableCollisionsClientRpc()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void EnableCollisionsServerRpc()
    {
        EnableCollisionsClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableCollisionsClientRpc()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = true;
        }
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
