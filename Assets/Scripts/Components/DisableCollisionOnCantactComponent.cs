using Unity.Netcode;
using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isCollidersEnabled = true;
    
    public bool IsCollidersEnabled => isCollidersEnabled;
    
    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
        EnableCollisions();
    }

    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        //if(!IsOwner) return;
        
        //DisableCollisionsServerRpc();
        
        DisableCollisions();
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
    
    public void DisableCollisions()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
        isCollidersEnabled = false;
    }

    public void EnableCollisions()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = true;
        }
        isCollidersEnabled = true;
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
