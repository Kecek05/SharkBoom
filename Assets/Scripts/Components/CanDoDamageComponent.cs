using System;
using UnityEngine;
using Unity.Netcode;

public class CanDoDamageComponent : NetworkBehaviour
{
    [SerializeField] private DamageableSO damageableSO;
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private bool canDoDamage;
    private bool damaged = false; //damage only once

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return; // Only the server should handle the damage
        baseCollisionController.OnCollided += baseCollisionController_OnItemCollided;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return; // Only the server should handle the damage
        baseCollisionController.OnCollided -= baseCollisionController_OnItemCollided;
    }

    private void baseCollisionController_OnItemCollided(GameObject collidedObj)
    {
        if (!canDoDamage) return;

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
}
