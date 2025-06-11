using System;
using UnityEngine;

public class CanDoDamageComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DamageableSO damageableSO;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private bool damaged = false; //damage only once

    private void OnEnable()
    {
        damaged = false;
        baseCollisionController.OnCollided += BaseCollisionController_OnItemCollided;
    }

    /*
    public override void OnNetworkSpawn()
    {
        damaged = false;
        baseCollisionController.OnCollided += BaseCollisionController_OnItemCollided;
    }*/

    private void BaseCollisionController_OnItemCollided(GameObject collidedObj)
    {
        //if(!IsOwner) return;
        
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

    public void SetDamageableSO(DamageableSO damageableSO)
    {
        this.damageableSO = damageableSO;
    }

    /*public override void OnNetworkDespawn()
    {
        if (!IsServer) return; // Only the server should handle the damage
        baseCollisionController.OnCollided -= BaseCollisionController_OnItemCollided;
    }*/

    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnItemCollided;
    }
}
