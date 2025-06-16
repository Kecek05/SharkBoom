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
    private bool isMeshVisible = true;
    
    public bool IsMeshVisible => isMeshVisible;
    
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
        HideMesh();
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        //if (!IsOwner) return;
       // HideMeshServerRpc();
       HideMesh();
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

    public void HideMesh()
    {
        meshToHide.SetActive(false);
        isMeshVisible = false;
    }

    public void ShowMesh()
    {
        meshToHide.SetActive(true);
        isMeshVisible = true;
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
