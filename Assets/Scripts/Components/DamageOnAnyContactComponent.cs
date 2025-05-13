using Unity.Netcode;
using UnityEngine;

public class DamageOnAnyContactComponent : NetworkBehaviour
{
    [SerializeField] private DamageableSO damageableSO;
    [SerializeField] private Collider2D[] itemColls2D;
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
            TurnOffCollServerRpc();

            damaged = true;
            damageable.TakeDamage(damageableSO);
        }
    }

    [Rpc(SendTo.Server)]
    private void TurnOffCollServerRpc()
    {
        TurnOffCollClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TurnOffCollClientRpc()
    {
        foreach (Collider2D itemCol in itemColls2D)
        {
            itemCol.enabled = false;
        }
        Debug.Log("Item Coll OFF");
    }
}
