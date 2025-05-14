using System;
using UnityEngine;
using Unity.Netcode;

public class CanDoDamageComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private DamageableSO damageableSO;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private bool damaged = false; //damage only once

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return; // Only the server should handle the damage
        baseCollisionController.OnCollided += BaseCollisionController_OnItemCollided;
    }

    private void BaseCollisionController_OnItemCollided(GameObject collidedObj)
    {
        if(collidedObj.TryGetComponent(out IDamageable damageable)) //Only on server
        {
            TakeDamage(damageable);
        }
    }

    public void TakeDamage(IDamageable damageable)
    {
        if (!damaged)
        {
            damaged = true;
            damageable.TakeDamage(damageableSO);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return; // Only the server should handle the damage
        baseCollisionController.OnCollided -= BaseCollisionController_OnItemCollided;
    }
}
