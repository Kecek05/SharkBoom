using Unity.Netcode;
using UnityEngine;

public class StuckInPlayerOnCollisionComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isFollowing = false;

    private void OnEnable()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
        ResetStuckState();
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject) 
    {
        if(!IsOwner) return;

        //Need to listen to OnCollided to follow the collided, not the player rot
        if (isFollowing) return;

        if (collidedObject.transform.parent == null) return;

        if (collidedObject.transform.parent.TryGetComponent(out PlayerThrower playerThrower)) //Get Component from the parent Obj, The Player Obj
        {
            //its a player
            isFollowing = true;
            followTransformComponent.SetTarget(playerThrower.ItemStuckSocket.transform);
            followTransformComponent.SetUseInterpolation(true);
            followTransformComponent.EnableComponent();
        }
    }

    [Rpc(SendTo.Server)]    
    private void ResetStuckStateServerRpc()
    {
        ResetStuckStateClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResetStuckStateClientRpc()
    {
        ResetStuckState();
    }

    private void ResetStuckState()
    {
        isFollowing = false;
        followTransformComponent.DisableComponent();
        followTransformComponent.SetUseInterpolation(false);
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }

}
