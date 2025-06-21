using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    private event Action OnDie;

    private event Action OnLocalDie;
    
    [BetterHeader("Settings")]
    [SerializeField] protected float maxHealth;
    protected NetworkVariable<float> currentHealth = new();
    protected float localCurrentHealth;

    protected NetworkVariable<bool> isDead = new(false);
    protected bool localIsDead = false;
    
    public bool LocalIsDead => localIsDead;
    
    public NetworkVariable<float> CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Only the server should be able to change the health
            currentHealth.Value = maxHealth;
        }
        
        UpdateLocalHealth(currentHealth.Value);
    }

    private void UpdateLocalHealth(float newValue)
    {
        localCurrentHealth = newValue;
        Debug.Log($"HEALTH - Changing Local Current Health: {localCurrentHealth} - {gameObject.name}");
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    protected void ModifyHealthServerRpc(float value) //only server
    {
        if (isDead.Value == true) return;

        float newHealth = currentHealth.Value + value;

        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        Debug.Log($"HEALTH - Health: {currentHealth.Value} - {gameObject.name}");

        if (currentHealth.Value <= 0)
        {
            isDead.Value = true;
            Die();
        }
    }
    
    protected void ModifyLocalHealth(float value)
    {
        float newHealth = localCurrentHealth + value;

        localCurrentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        InvokeOnTakeLocalDamage();
        Debug.Log($"HEALTH - Local Health: {localCurrentHealth} - Recieved Value: {value} - {gameObject.name}");

        if (localCurrentHealth <= 0)
        {
            localIsDead = true;
            OnLocalDie?.Invoke();
        }
    }

    protected virtual void InvokeOnTakeLocalDamage()
    {
        
    }
    
    [Command("health-die")]
     protected virtual void Die()
     {
        if(!IsServer) return;
        
        OnDie?.Invoke();
        
     } 
}
