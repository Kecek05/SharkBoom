using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class StuckInPlayerOnCollisionNetworkedComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isFollowing = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject)
    {
        if (!IsOwner) return;

        if(isFollowing) return; //Already following a player

        if (collidedObject.gameObject.TryGetComponent(out PlayerThrower playerThrower))
        {
            //its a player
            isFollowing = true;
            followTransformComponent.SetTarget(collidedObject.transform);
            StartCoroutine(DelayToStuck());
        }
    }

    private IEnumerator DelayToStuck()
    {
        //Delay to Stuck in the player
        yield return new WaitForSeconds(0.2f);
        followTransformComponent.EnableComponent();
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }
}
