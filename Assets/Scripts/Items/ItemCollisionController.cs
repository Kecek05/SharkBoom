using System;
using Unity.Netcode;
using UnityEngine;


public class ItemCollisionController : BaseCollisionController
{

    protected override void HandleCollision(GameObject collidedObject)
    {
        TriggerOnCollided(collidedObject);

        if (collidedObject.TryGetComponent(out PlayerThrower playerThrower))
        {
            TriggerOnCollidedWithPlayer(playerThrower);
        }
    }
}
