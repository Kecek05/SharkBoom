using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;


    protected override void ItemReleased(float force, Vector3 direction)
    {
        base.ItemReleased(force, direction);

        objectToFollowTransform = PlayersPublicInfoManager.Instance.GetPlayerObjectByPlayableState(thisItemLaucherData.ownerPlayableState).transform;

        if (isServerObject)
            StartCoroutine(PlayerFollowFirework());
    }

    protected override void ItemCallbackAction()
    {
        if (!isServerObject) return; // Only the server should call the callback action

        GameFlowManager.Instance.PlayerJumpedServerRpc(thisItemLaucherData.ownerPlayableState);

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
