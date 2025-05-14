using System;
using Unity.Netcode;
using UnityEngine;


public class ItemCollisionController : BaseCollisionController
{

    [Header("Reference")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private DisableCollisionOnCantactComponent itemCollisionDisablerComponent;
    [SerializeField] private GameObject meshToHide;
    [SerializeField] private Vector3 knockback;
    [Space(5)]

    [Header("Settings")]
    [SerializeField] private bool canDetectCollisionWithPlayer;
    [SerializeField] private bool canDoDamage;
    [SerializeField] private bool canDoKnockback;
    [SerializeField] private bool canDisableColliders;
    [SerializeField] private bool canFollowTransform;

    protected override void HandleCollision(GameObject collidedObject)
    {
        TriggerOnCollided(collidedObject);

        if (collidedObject.TryGetComponent(out PlayerThrower playerThrower))
        {
            TriggerOnCollidedWithPlayer(playerThrower);
        }
    }
}
