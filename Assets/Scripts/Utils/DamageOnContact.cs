using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage;
    public float Damage => damage;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.rigidbody.TryGetComponent(out IDamageable damageableObject))
        {
            damageableObject.DealDamage(damage);
            Debug.Log("Dealt " + damage + " damage to " + collision.gameObject.name);
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }


}
