using UnityEngine;


public class ItemCollisionController : BaseCollisionController
{

    protected override void HandleCollision(Collider collidedObject)
    {
        Debug.Log($"collidedObj: {collidedObject} - Obj Parent: {collidedObject.transform.parent} - My Obj layer: {gameObject.layer}");
        TriggerOnCollided(collidedObject.gameObject); // Get the component from the collided object, head, body or foot.

        if (collidedObject.transform.parent == null) return; 

        if (collidedObject.transform.parent.TryGetComponent(out PlayerThrower playerThrower)) //Get Component from the parent Obj, The Player Obj
        {
            TriggerOnCollidedWithPlayer(playerThrower);
        }
    }
}
