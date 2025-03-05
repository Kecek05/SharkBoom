using Unity.Netcode;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage;
    public float Damage => damage;
    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                damageable.TakeDamage(damage);
                Debug.Log("Dealt " + damage + " damage to " + collision.gameObject.name);
            }
        }
        Destroy(gameObject);

    }

}
