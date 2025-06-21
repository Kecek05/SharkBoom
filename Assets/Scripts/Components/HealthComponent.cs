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
        
        currentHealth.OnValueChanged += OnValueChanged;
        OnValueChanged(0f, currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnValueChanged;
    }

    private void OnValueChanged(float previousValue, float newValue)
    {
        localCurrentHealth = newValue;
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    protected void ModifyHealthServerRpc(float value) //only server
    {
        if (isDead.Value == true) return;

        float newHealth = currentHealth.Value + value;

        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        Debug.Log($"Health: {currentHealth.Value}");

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
        Debug.Log($"Health: {localCurrentHealth}");

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
