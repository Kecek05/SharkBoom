using System;
using System.Collections;
using UnityEngine;

public class JumpItemThrowable : BaseItemThrowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;
    private float lockedZ;
    private Coroutine followCoroutine;

    private BaseTimerManager timerManager;

    private void Start()
    {
        timerManager = ServiceLocator.Get<BaseTimerManager>();
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        if (!IsOwner) return;

        objectToFollowTransform = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(thisItemLaucherData.ownerPlayableState).transform;
        lockedZ = objectToFollowTransform.position.z;

        followCoroutine ??= StartCoroutine(PlayerFollowJump());
    }

    protected override void ItemCallbackAction()
    {
        if (!IsOwner) return;

        timerManager.TogglePauseTimer(false); //unpause
        turnManager.PlayerJumped(thisItemLaucherData.ownerPlayableState);
        FireItemCallbackAction();
    }

    private IEnumerator PlayerFollowJump()
    {
        if (objectToFollowTransform == null) yield break;

        currentFollowingTime = 0f;
        while (currentFollowingTime < followingTime)
        {
            Vector3 itemPos = transform.position;
            objectToFollowTransform.position =
                new Vector3(itemPos.x, itemPos.y, lockedZ);

            currentFollowingTime += Time.deltaTime;
            yield return null;
        }
    }

    protected override void ResetItemThrowableState()
    {
        base.ResetItemThrowableState();

        objectToFollowTransform = null;

        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
}
