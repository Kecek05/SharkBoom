using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        if (isServerObject) return; // Jump is Client Sided. The server should not follow the player

        objectToFollowTransform = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(thisItemLaucherData.ownerPlayableState).transform;

        turnManager = ServiceLocator.Get<BaseTurnManager>();

        StartCoroutine(PlayerFollowFirework());
    }

    protected override void ItemCallbackAction()
    {
        if (!isServerObject) return;

        turnManager.PlayerJumped(thisItemLaucherData.ownerPlayableState);

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
