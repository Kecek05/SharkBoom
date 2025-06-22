using Unity.Netcode;
using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isCollidersEnabled = true;
    
    public bool IsCollidersEnabled => isCollidersEnabled;
    
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
        isCollidersEnabled = false;
    }

    public void EnableCollisions()
    {
        foreach (Collider itemCol in itemColliders)
        {
            itemCol.enabled = true;
        }
        isCollidersEnabled = true;
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
