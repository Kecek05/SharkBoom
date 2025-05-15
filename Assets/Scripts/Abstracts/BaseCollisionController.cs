using System;
using UnityEngine;

public abstract class BaseCollisionController : MonoBehaviour
{
    /// <summary>
    /// Called when the item is collided with player. Pass the PlayerThrower component.
    /// </summary>
    public event Action<PlayerThrower> OnCollidedWithPlayer;
    /// <summary>
    /// lled when the item is collided with any object. Pass the Collided object.
    /// </summary>
    public event Action<GameObject> OnCollided;

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    protected abstract void HandleCollision(Collider2D collidedObject);

    protected void TriggerOnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        OnCollidedWithPlayer?.Invoke(playerThrower);
    }

    protected void TriggerOnCollided(GameObject collidedObject)
    {
        OnCollided?.Invoke(collidedObject);
    }
}
