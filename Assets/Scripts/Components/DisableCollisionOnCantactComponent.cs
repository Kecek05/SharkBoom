using Unity.Netcode;
using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
        EnableCollisions();
    }

    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        //if(!IsOwner) return;
        
        //DisableCollisionsServerRpc();
        
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
    }

    /*[Rpc(SendTo.Server)]
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
    }*/

    private void EnableCollisions()
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
