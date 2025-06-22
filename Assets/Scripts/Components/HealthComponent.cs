using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class HealthComponent : NetworkBehaviour
{

    public static event Action OnLocalDie;

    /// <summary>
    /// Called when LocalHealth is synced with network. Pass the LocalHealth
    /// </summary>
    public event Action<float> OnLocalHealthSynced;
    
    [BetterHeader("Settings")]
    [SerializeField] protected float maxHealth;
    protected NetworkVariable<float> currentHealth = new();
    protected float localCurrentHealth;
    /// <summary>
    /// Health that localCurrentHealth must be at the end of the round. This is synced with Current Health
    /// </summary>
    protected float localTargetHealth; 

    protected NetworkVariable<bool> isDead = new(false);
    
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
        localTargetHealth = newValue;
        // Debug.Log($"HEALTH - Changing Local Current Health: {localCurrentHealth} - Local Target Health: {localTargetHealth} - {gameObject.name}");
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    protected void ModifyHealthServerRpc(float value) //only server
    {
        if (isDead.Value == true) return;

        float newHealth = currentHealth.Value + value;

        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        // Debug.Log($"HEALTH - Health: {currentHealth.Value} - {gameObject.name}");

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

        InvokeOnLocalHealthChanged();
        // Debug.Log($"HEALTH - Local Health: {localCurrentHealth} - Recieved Value: {value} - {gameObject.name}");
    }

    protected void SetLocalHealth(float value)
    {
        localCurrentHealth = Mathf.Clamp(value, 0, maxHealth);
        InvokeOnLocalHealthChanged();
        OnLocalHealthSynced?.Invoke(localCurrentHealth);
        // Debug.Log($"HEALTH - Set LocalHealth to: {localCurrentHealth} - {gameObject.name}");
    }

    protected void InvokeOnLocalDie()
    {
        // Debug.Log($"HEALTH - Local Die Triggered");
        OnLocalDie?.Invoke();
    }

    protected abstract void InvokeOnLocalHealthChanged();

    [Command("health-die")]
    protected abstract void Die();
}
