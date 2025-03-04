using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    //Events
    public event Action OnDie;


    [BetterHeader("Settings")]
    [SerializeField] private float maxHealth;
    private NetworkVariable<float> currentHealth = new();

    private bool isDead = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only the server should be able to change the health

        currentHealth.Value = maxHealth;
    }

    [Command("health-takeDamage")]
    public void TakeDamage(float damage)
    {
        if(!IsServer) return;

        if(isDead) return;

        ModifyHealth(-damage);
    }


    [Command("health-heal")]
    public void Heal(float healthToHeal) //only server
    {
        if (!IsServer) return;

        if (isDead) return;

        ModifyHealth(healthToHeal);

        currentHealth.Value += healthToHeal;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }
    }


    private void ModifyHealth(float value) //only server
    {
        if (!IsServer) return;

        if (isDead) return;

        float newHealth = currentHealth.Value + value;

        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        Debug.Log($"Health: {currentHealth.Value}");

        if (currentHealth.Value <= 0)
        {
            isDead = true;
            Die();
        }
    }

    [Command("health-die")]
    public void Die()
    {
        if(!IsServer) return;

        OnDie?.Invoke();
        //Temp, after will only invoke the event 

        if(gameObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
    }

}
