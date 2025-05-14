using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CanDoDamageComponent), typeof(FollowTransformComponent), typeof(ItemCollisionDisablerComponent))]
public class ItemCollisionController : NetworkBehaviour
{
    /// <summary>
    /// Called when the item is collided with player and the GFX is Hidded.
    /// </summary>
    public event Action OnItemCollidedWithPlayer;

    [SerializeField] private CanDoDamageComponent canDoDamage;
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private ItemCollisionDisablerComponent itemCollisionDisablerComponent;
    [SerializeField] private GameObject meshToHide;
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
        if (collidedObject.TryGetComponent(out IDamageable damageable) && IsServer)
        {
            canDoDamage.TakeDamage(damageable);
        }

        if (collidedObject.TryGetComponent(out IRecieveKnockback knockable))
        {
            //Hitted a player
            meshToHide.SetActive(false);
            OnItemCollidedWithPlayer?.Invoke();
            knockable.DoOnRecieveKnockback(knockback, transform.position);
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


        
    }
}
