using System;
using UnityEngine;

public class KnockbackTriggerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [Space(5)]

    [Header("Settings")]
    [SerializeField] private float knockbackStrength;

    private void OnEnable()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if (collidedObject.transform.parent == null) return; //Check if the collided object has a parent

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveKnockback knockbackReceiver)) //Call on the parent
        {
            knockbackReceiver.DoOnRecieveKnockback(knockbackStrength, transform.position); //Pass the pos of the object that triggered
        }
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
