using System;
using System.Collections;
using UnityEngine;

public class JumpItem : BaseItemThrowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;

    private BaseTimerManager timerManager;

    private void Start()
    {
        timerManager = ServiceLocator.Get<BaseTimerManager>();
    }

    public override void Initialize(Transform parent)
    {
        base.Initialize(parent);
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        if (!IsOwner) return;

        objectToFollowTransform = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(thisItemLaucherData.ownerPlayableState).transform;

        StartCoroutine(PlayerFollowJump());
    }

    protected override void ItemCallbackAction()
    {
        if (!IsOwner) return;

        timerManager.TogglePauseTimer(false); //unpause
        turnManager.PlayerJumped(thisItemLaucherData.ownerPlayableState);

    }

    private IEnumerator PlayerFollowJump()
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

    public override void DestroyItem(Action destroyedCallback = null)
    {
        base.DestroyItem(destroyedCallback);

        objectToFollowTransform = null;
    }
}
