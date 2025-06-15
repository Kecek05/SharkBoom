using System;
using Unity.Netcode;
using UnityEngine;

public class HideMeshOnCollisionComponent : MonoBehaviour
{
    public event Action OnMeshHidden;

    [Header("References")]
    [SerializeField] private BaseCollisionController baseCollisionController;
    [SerializeField] private GameObject meshToHide;
    [Space(5)]

    [Header("Settings")]
    [SerializeField] private bool hideOnCollisionWithPlayer = true;
    [SerializeField] private bool hideOnCollisionWithAnything = true;
    private void OnEnable()
    {
        if (hideOnCollisionWithAnything)
        {
            baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
        }

        if (hideOnCollisionWithPlayer)
        {
            baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
        }
        ShowMesh();
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        //if (!IsOwner) return;
        //HideMeshServerRpc();
        meshToHide.SetActive(false);
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        //if (!IsOwner) return;
       // HideMeshServerRpc();
        meshToHide.SetActive(false);
    }

    /*[Rpc(SendTo.Server)]
    private void HideMeshServerRpc()
    {
        HideMeshClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideMeshClientRpc()
    {
        meshToHide.SetActive(false);
    }*/

    private void ShowMesh()
    {
        meshToHide.SetActive(true);
    }

    private void OnDisable()
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
