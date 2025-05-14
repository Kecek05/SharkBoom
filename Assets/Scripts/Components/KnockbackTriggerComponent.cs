using System;
using UnityEngine;

public class KnockbackTriggerComponent : MonoBehaviour
{
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private Vector3 knockbackForce;

    private void Start()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if(collidedObject.TryGetComponent(out IRecieveKnockback knockbackReceiver))
        {
            knockbackReceiver.DoOnRecieveKnockback(knockbackForce, transform.position, null); //get the rigidbody somehow
        }
    }

    private void OnDestroy()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
