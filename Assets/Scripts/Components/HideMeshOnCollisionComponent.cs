using System;
using UnityEngine;

public class HideMeshOnCollisionComponent : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private GameObject meshToHide;
    [Header("Settings")]
    [SerializeField] private bool hideOnCollisionWithPlayer = true;
    [SerializeField] private bool hideOnCollisionWithAnything = true;

    private void Start()
    {
        if(hideOnCollisionWithAnything)
        {
            baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
        }


        if(hideOnCollisionWithPlayer)
        {
            baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
        }
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        HideMesh();
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        HideMesh();
    }

    private void HideMesh()
    {
        meshToHide.SetActive(false);
    }

    private void OnDestroy()
    {
        if (hideOnCollisionWithAnything)
        {
            baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
        }


        if (hideOnCollisionWithPlayer)
        {
            baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
        }
    }
}
