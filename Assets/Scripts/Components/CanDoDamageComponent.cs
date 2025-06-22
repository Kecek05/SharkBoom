using System;
using UnityEngine;

public class CanDoDamageComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DamageableSO damageableSO;
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private BaseItemThrowable baseItemThrowable;
    private bool damaged = false; //damage only once
    private bool localDamaged = false;
    private DamageableSO selectedDamageableSO;
    
    private void OnEnable()
    {
        selectedDamageableSO = damageableSO;
        damaged = false;
        localDamaged = false;
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
        Debug.Log($"BaseCollisionController_OnItemCollided: {collidedObj.name}");
        //if(!IsOwner) return;
        if (baseItemThrowable.IsOwner)
        {
            if(collidedObj.TryGetComponent(out IDamageable damageable)) //Only on server
            {
                if(baseItemThrowable.IsOwner)
                    TakeDamage(damageable);
                
            }  
        }
        
        if(collidedObj.TryGetComponent(out ILocalDamageable localDamageable))
        {
            LocalTakeDamage(localDamageable);
        }
    }

    public void LocalTakeDamage(ILocalDamageable localDamageable)
    {
        if (!localDamaged)
        {
            localDamaged = true;
            localDamageable.TakeLocalDamage(selectedDamageableSO);
        }
    }

    public void TakeDamage(IDamageable damageable)
    {
        if (!damaged)
        {
            damaged = true;
            damageable.TakeDamage(selectedDamageableSO);
        }
    }

    public void SetDamageableSO(DamageableSO damageableSO)
    {
        selectedDamageableSO = damageableSO;
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnItemCollided;
    }
}
