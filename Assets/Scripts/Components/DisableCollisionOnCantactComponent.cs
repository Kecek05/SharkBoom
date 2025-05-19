using System;
using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;

    private void Start()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
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


    private void OnDestroy()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
