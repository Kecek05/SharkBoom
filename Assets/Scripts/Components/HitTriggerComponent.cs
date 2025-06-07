using System;
using Unity.Netcode;
using UnityEngine;

public class HitTriggerComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackStrength;
    

    private void OnEnable()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if (collidedObject.transform.parent == null) return; //Check if the collided object has a parent

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveHit recieveHit)) //Call on the parent
        {
            recieveHit.Hit();
        }

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveKnockback _))
        {
            //Collided with a Implemented IRecieveKnockback interface, so we can apply knockback
            if(!IsOwner) return;
            if (collidedObject.transform.parent.TryGetComponent(out NetworkObject collidedNetworkObject))
            {
                //Get the NetworkObject to pass to other clients
                DoKnockbackServerRpc(collidedNetworkObject, transform.position);
            }
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void DoKnockbackServerRpc(NetworkObjectReference collidedNetworkObjectReference,  Vector3 hitPosition)
    {
        DoKnockbackClientRpc(collidedNetworkObjectReference, hitPosition);
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void DoKnockbackClientRpc(NetworkObjectReference collidedNetworkObjectReference,  Vector3 hitPosition)
    {
        DoKnockback(collidedNetworkObjectReference, hitPosition);
    }
    
    private void DoKnockback(NetworkObjectReference collidedNetworkObjectReference,  Vector3 hitPosition)
    {
        if (collidedNetworkObjectReference.TryGet(out NetworkObject collidedNetworkObject))
        {
            if (collidedNetworkObject.TryGetComponent(out IRecieveKnockback receiver))
            {
                receiver.DoOnRecieveKnockback(knockbackStrength, hitPosition); //Pass the pos of the object that triggered
            }
        }
    }
    
    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
