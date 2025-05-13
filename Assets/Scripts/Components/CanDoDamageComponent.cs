using Unity.Netcode;
using UnityEngine;

public class CanDoDamageComponent : NetworkBehaviour
{
    [SerializeField] private DamageableSO damageableSO;
    private bool damaged = false; //damage only once

    public void TakeDamage(IDamageable damageable)
    {
        if (!damaged)
        {
            damaged = true;
            damageable.TakeDamage(damageableSO);
        }
    }
}
