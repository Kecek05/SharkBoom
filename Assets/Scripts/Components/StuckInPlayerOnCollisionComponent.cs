using System.Collections;
using UnityEngine;

public class StuckInPlayerOnCollisionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FollowTransformComponent followTransformComponent;
    [SerializeField] private BaseCollisionController baseCollisionController;
    private bool isFollowing = false;

    private void OnEnable()
    {
        baseCollisionController.OnCollided += BaseCollisionController_OnCollided;
    }

    private void BaseCollisionController_OnCollided(GameObject collidedObject) 
    {
        //Need to listen to OnCollided to follow the collided, not the player rot
        if (isFollowing) return;

        if (collidedObject.transform.parent == null) return;

        if (collidedObject.transform.parent.TryGetComponent(out PlayerThrower playerThrower)) //Get Component from the parent Obj, The Player Obj
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

    private void ResetStuckState()
    {
        isFollowing = false;
        followTransformComponent.DisableComponent();
    }

    private void OnDisable()
    {
        baseCollisionController.OnCollided -= BaseCollisionController_OnCollided;
        ResetStuckState();
    }

}
