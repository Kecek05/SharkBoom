using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CanDoDamageComponent), typeof(FollowTransformComponent), typeof(ItemCollisionDisablerComponent))]
public class ItemCollisionController : NetworkBehaviour
{
    [SerializeField] private CanDoDamageComponent canDoDamage;
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private ItemCollisionDisablerComponent itemCollisionDisablerComponent;
    [SerializeField] private GameObject gfx;
    [SerializeField] private Vector3 knockback;

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
            canDoDamage.TakeDamage(damageable);
        }

        if (collidedObject.TryGetComponent(out IRecieveKnockback knockable))
        {
            knockable.DoOnRecieveKnockback(knockback, collidedObject.transform.position);
        }

        if(itemCollisionDisablerComponent != null)
        {
            itemCollisionDisablerComponent.DisableCollisions();
        }

        if(IsOwner)
        {
            followTransformComponent.SetTarget(collidedObject.transform);
            followTransformComponent.SetFollowRotation(false);
            followTransformComponent.EnableComponent();
        }


        gfx.SetActive(false);
    }
}
