using Unity.Netcode;
using UnityEngine;

public class FreezeOnCollisionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
        //UnfreezeObject();
        rb.isKinematic = false; // Unfreeze the object
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        //if(!IsOwner) return;
        //FreezeObjectServerRpc();
        rb.isKinematic = true; // Freeze the object
    }

    /*
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
    */

    private void UnfreezeObject()
    {
        rb.isKinematic = false; // Unfreeze the object
    }
    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }

}
