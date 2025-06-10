using System;
using UnityEngine;

public abstract class BaseCollisionController : MonoBehaviour
{
    /// <summary>
    /// Called when the item is collided with player. Pass the PlayerThrower component.
    /// </summary>
    public event Action<PlayerThrower> OnCollidedWithPlayer;
    /// <summary>
    /// Called when the item is collided with any object. Pass the Collided object.
    /// </summary>
    public event Action<GameObject> OnCollided;
    
    /// <summary>
    /// Called when the item is collided with any object except player. Pass the Collided object.
    /// </summary>
    public event Action<GameObject> OnCollidedWithoutPlayer;

    protected void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }

    protected void OnTriggerEnter(Collider collision)
    {
        HandleCollision(collision);
    }

    protected abstract void HandleCollision(Collider collidedObject);

    protected void TriggerOnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        OnCollidedWithPlayer?.Invoke(playerThrower);
    }

    protected void TriggerOnCollided(GameObject collidedObject)
    {
        OnCollided?.Invoke(collidedObject);
    }

    protected void TriggerOnCollidedWithoutPlayer(GameObject collidedObject)
    {
        OnCollidedWithoutPlayer?.Invoke(collidedObject);
    }
}
