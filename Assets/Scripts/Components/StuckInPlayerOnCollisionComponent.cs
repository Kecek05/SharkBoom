using System.Collections;
using UnityEngine;

public class StuckInPlayerOnCollisionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isFollowing = false;

    private void Start()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject) //Need to listen to OnCollided to follow the collided, not the player rot
    {

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

    private void OnDestroy()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
    }

}
