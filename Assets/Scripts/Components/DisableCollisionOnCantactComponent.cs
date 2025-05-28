using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void OnEnable()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;

        EnableCollisions();
    }


    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        DisableCollisions();
    }

    public void DisableCollisions()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
    }

    public void EnableCollisions()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = true;
        }
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
