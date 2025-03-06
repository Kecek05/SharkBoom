using Unity.Netcode;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private DamageableSO damageableSO;

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                damageable.TakeDamage(damageableSO);
                Debug.Log("Dealt " + damageableSO.damage + " damage to " + collision.gameObject.name);
            }
        }
        Destroy(gameObject);

    }

}
