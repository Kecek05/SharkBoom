using QFSW.QC;
using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private event Action OnDie;

    [BetterHeader("Settings")]
    [SerializeField] protected float maxHealth;
    protected NetworkVariable<float> currentHealth = new();

    protected bool isDead = false;

    public NetworkVariable<float> CurrentHealth => currentHealth;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only the server should be able to change the health

        currentHealth.Value = maxHealth;

    }


    [Command("health-heal")]
    protected void Heal(float healthToHeal) //only server
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


    protected void ModifyHealth(float value) //only server
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
     protected virtual void Die()
     {
        if(!IsServer) return;

        //Temp, after will only invoke the event 

        OnDie?.Invoke();

          //if (gameObject.TryGetComponent(out NetworkObject networkObject))
          //{
          //    networkObject.Despawn(true);
          //}
     } 

}
