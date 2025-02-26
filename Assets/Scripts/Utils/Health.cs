using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    private NetworkVariable<float> currentHealth = new();

    private void Start()
    {
        currentHealth.Value = maxHealth;
    }

    [Command("health-dealDamage")]
    public void DealDamage(float damage)
    {
        LoseHealthServerRpc(damage);
    }

    [Rpc(SendTo.Server)]
    private void LoseHealthServerRpc(float healthToLose)
    {
        currentHealth.Value -= healthToLose;
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    [Rpc(SendTo.Server)]
    private void GainHealthServerRpc(float healthToGain)
    {
        currentHealth.Value += 10;
        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }
    }


    public void Die()
    {
        Destroy(gameObject);
    }




}
