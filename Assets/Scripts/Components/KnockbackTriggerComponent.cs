using System;
using UnityEngine;

public class KnockbackTriggerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [Space(5)]

    [Header("Settings")]
    [SerializeField] private Vector3 knockbackForce;

    private void Start()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if(collidedObject.TryGetComponent(out IRecieveKnockback knockbackReceiver))
        {
            //Not working yet. Need to make a way to get the rigidbody of the collided object
            //knockbackReceiver.DoOnRecieveKnockback(knockbackForce, transform.position, null); //get the rigidbody somehow
        }
    }

    private void OnDestroy()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
