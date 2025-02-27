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
        if(isDead) return;

        if(!IsServer) return;

        ModifyHealth(-damage);
    }


    [Command("health-heal")]
    public void Heal(float healthToHeal)
    {
        if(isDead) return;

        ModifyHealth(healthToHeal);

        currentHealth.Value += healthToHeal;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }
    }


    private void ModifyHealth(float value)
    {
        if(isDead) return;

        float newHealth = currentHealth.Value + value;

        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            isDead = true;
            Die();
        }
    }

    [Command("health-die")]
    public void Die()
    {
        OnDie?.Invoke();
        Destroy(gameObject);
    }




}
