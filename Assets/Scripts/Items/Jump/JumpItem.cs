using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable, IFollowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;

    public void Follow(Transform objectToFollow)
    {
        objectToFollowTransform = objectToFollow;
    }


    protected override void ItemReleased(float force, Vector3 direction)
    {
        base.ItemReleased(force, direction);
        StartCoroutine(PlayerFollowFirework());
    }

    protected override void ItemCallbackAction()
    {
        if (!isServerObject) return; // Only the server should call the callback action

        GameFlowManager.Instance.PlayerJumped(ownerPlayableState);

        //GameFlowManager.Instance.ItemFinishOurAction();

    }

    private IEnumerator PlayerFollowFirework()
    {
        if(objectToFollowTransform == null) yield break; // if object to follow is null, isnt the owner

        currentFollowingTime = 0f;
        while (currentFollowingTime < followingTime)
        {
            objectToFollowTransform.position = transform.position;
            currentFollowingTime += Time.deltaTime;
            yield return null;
        }
    }
}
