using Unity.Netcode;
using UnityEngine;

public class ItemCollisionController : NetworkBehaviour
{
    [SerializeField] private DamageOnAnyContactComponent damageOnAnyContactComponent;
    [SerializeField] private ItemCollisionDisablerComponent itemCollisionDisablerComponent;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision2D(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        HandleCollision2D(collision.gameObject);
    }

    private void HandleCollision2D(GameObject collidedObject)
    {
        if (collidedObject.TryGetComponent(out IDamageable damageable) || IsServer)
        {
            damageOnAnyContactComponent.TakeDamage(damageable);
        }

        if (collidedObject.TryGetComponent(out IRecieveKnockback knockable))
        {
            knockable.DoOnRecieveKnockback(new Vector3(100,0,0), transform.position); // need to get the contact point TODO
        }

        if(itemCollisionDisablerComponent != null)
        {
            itemCollisionDisablerComponent.DisableCollisions();
        }
    }
}
