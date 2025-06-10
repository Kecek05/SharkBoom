using UnityEngine;


public class ItemCollisionController : BaseCollisionController
{

    protected override void HandleCollision(Collider collidedObject)
    {
        TriggerOnCollided(collidedObject.gameObject); // Get the component from the collided object, head, body or foot.

        if (!collidedObject.transform.parent)
        {
            TriggerOnCollidedWithoutPlayer(collidedObject.gameObject);
            return;
        } 

        if (collidedObject.transform.parent.TryGetComponent(out PlayerThrower playerThrower)) //Get Component from the parent Obj, The Player Obj
        {
            TriggerOnCollidedWithPlayer(playerThrower);
        }
        else
        {
            TriggerOnCollidedWithoutPlayer(collidedObject.gameObject);
        }
    }
}
