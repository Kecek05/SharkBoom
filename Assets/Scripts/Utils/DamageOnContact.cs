using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.rigidbody.TryGetComponent(out IDamageable damageableObject))
        {
            damageableObject.DealDamage(1);
        }
    }
}
