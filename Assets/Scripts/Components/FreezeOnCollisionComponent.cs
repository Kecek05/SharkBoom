using UnityEngine;

public class FreezeOnCollisionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void Start()
    {
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        Debug.Log("Stop moving");
        rb.isKinematic = true; // Freeze the object
    }

    private void OnDestroy()
    {
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }
}
