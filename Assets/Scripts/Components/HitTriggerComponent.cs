using UnityEngine;

public class HitTriggerComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;


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
    }

    private void OnDestroy()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
