using UnityEngine;

public class HitTriggerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackStrength;

    private void Start()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if (collidedObject.transform.parent == null) return; //Check if the collided object has a parent

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveHit recieveHit)) //Call on the parent
        {
            recieveHit.Hit();
        }

        if (collidedObject.transform.parent.TryGetComponent(out IRecieveKnockback knockbackReceiver))
        {
            knockbackReceiver.DoOnRecieveKnockback(knockbackStrength, transform.position); //Pass the pos of the object that triggered
        }
    }

    private void OnDestroy()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
