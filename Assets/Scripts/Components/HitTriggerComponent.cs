using System;
using Sortify;
using Unity.Netcode;
using UnityEngine;

public class HitTriggerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [BetterHeader("Settings")]
    [SerializeField] private bool isJump;
    [Header("Knockback Settings")]
    [SerializeField] private KnockbackSO knockback;
    private KnockbackSO selectedKnockbackSO; // the SO that is currencly been used

    private void OnEnable()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
        selectedKnockbackSO = knockback;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        //if(!IsOwner) return;
        
        if (!collidedObject.transform.parent) return; //Check if the collided object has a parent

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveHit recieveHit)) //Call on the parent
        {
            recieveHit.Hit(isJump);
        }

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveKnockback knockbackReceiver))
        {
            knockbackReceiver.DoOnRecieveKnockback(selectedKnockbackSO.knockbackStrength, transform.position); //Pass the pos of the object that triggered
        }
    }
    
    public void SetKnockbackSO(KnockbackSO knockbackSoActivated)
    {
        selectedKnockbackSO = knockbackSoActivated;
    }
    
    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }


}
