using Unity.Netcode;
using UnityEngine;

public class DamageOnAnyContactComponent : NetworkBehaviour
{
    [SerializeField] private DamageableSO damageableSO;
    private bool damaged = false; //damage only once

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            TakeDamage(damageable);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            TakeDamage(damageable);
        }
    }


    private void TakeDamage(IDamageable damageable)
    {
        if (!damaged)
        {
            damaged = true;
            damageable.TakeDamage(damageableSO);
        }
    }
}
