using UnityEngine;

public class FreezeOnCollisionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        Debug.Log("Stop moving");
        FreezeObject();
    }

    private void FreezeObject()
    {
        rb.isKinematic = true; // Freeze the object
    }

    private void UnfreezeObject()
    {
        rb.isKinematic = false; // Unfreeze the object
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
        UnfreezeObject(); // Unfreeze the object when disabled
    }

}
