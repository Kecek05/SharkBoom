using System;
using UnityEngine;

public class DisableCollisionOnCantactComponent : MonoBehaviour
{
    [SerializeField] private Collider2D[] itemColliders;
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private bool canDisableCollisions;

    private void Start()
    {
        baseCollisionController.OnCollidedWithPlayer += HandleItemCollidedWithPlayer;
    }


    private void HandleItemCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if(!canDisableCollisions) return;

        DisableCollisions();
    }

    public void DisableCollisions()
    {
        foreach (Collider2D itemCol in itemColliders)
        {
            itemCol.enabled = false;
        }
    }


    private void OnDestroy()
    {
        baseCollisionController.OnCollidedWithPlayer -= HandleItemCollidedWithPlayer;
    }
}
