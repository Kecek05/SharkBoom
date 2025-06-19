using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    private event Action OnDie;

    [BetterHeader("Settings")]
    [SerializeField] protected float maxHealth;
    protected NetworkVariable<float> currentHealth = new();

    protected NetworkVariable<bool> isDead = new(false);

    public NetworkVariable<float> CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only the server should be able to change the health

        currentHealth.Value = maxHealth;

    }


    [Command("health-heal")]
    protected void Heal(float healthToHeal) //only server
    {
        if (!IsServer) return;

        if (isDead.Value) return;

        ModifyHealthServerRpc(healthToHeal);

        currentHealth.Value += healthToHeal;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }
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
    
    [Command("health-die")]
     protected virtual void Die()
     {
        if(!IsServer) return;
        
        OnDie?.Invoke();
        
     } 
}
