using Unity.Netcode;
using UnityEngine;

public class StuckInPlayerOnCollisionNetworkedComponent : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private BaseCollisionController baseCollisionController;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        baseCollisionController.OnCollidedWithPlayer += BaseCollisionController_OnCollidedWithPlayer;
    }

    private void BaseCollisionController_OnCollidedWithPlayer(PlayerThrower playerThrower)
    {
        if(!IsOwner) return;

        followTransformComponent.SetTarget(playerThrower.transform);
        followTransformComponent.EnableComponent();
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;
        baseCollisionController.OnCollidedWithPlayer -= BaseCollisionController_OnCollidedWithPlayer;
    }
}
