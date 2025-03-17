using Unity.Netcode;
using UnityEngine;

public class DamageOnAnyContact : MonoBehaviour
{
    [SerializeField] private DamageableSO damageableSO;
    private bool damaged = false; //damage only once

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer && !damaged)
            {
                damaged = true;
                damageable.TakeDamage(damageableSO);
                Debug.Log("Dealt " + damageableSO.damage + " damage to " + collision.gameObject.name);
            }
        }
    }
}
