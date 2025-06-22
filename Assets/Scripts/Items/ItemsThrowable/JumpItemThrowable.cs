using System.Collections;
using UnityEngine;

public class JumpItemThrowable : BaseItemThrowable
{

    [SerializeField] private float followingTime = 1.5f;
    private float currentFollowingTime = 0f;
    private Transform objectToFollowTransform;
    private float lockedZ;
    private Coroutine followCoroutine;
    private float followSpeed = 10f;

    private BaseTimerManager timerManager;

    private void Start()
    {
        timerManager = ServiceLocator.Get<BaseTimerManager>();
    }

    public override void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        base.ItemReleased(itemLauncherData, isOwner);

       // if (!IsOwner) return;

        objectToFollowTransform = ServiceLocator.Get<BasePlayersPublicInfoManager>().GetPlayerObjectByPlayableState(thisItemLaucherData.ownerPlayableState).transform;
        Debug.Log($"JUMP OBJECT TO FOLLOW: {objectToFollowTransform} - Owner: {thisItemLaucherData.ownerPlayableState}");
        lockedZ = objectToFollowTransform.position.z;
        
        followCoroutine ??= StartCoroutine(PlayerFollowJump());
        
        
    }

    protected override void ItemCallbackAction()
    {
        InvokeOnItemCallbackAction();
        
        if(isOwner) 
            timerManager.TogglePauseTimer(false); //unpause
        
        turnManager.PlayerJumped(thisItemLaucherData.ownerPlayableState);
    }

    private IEnumerator PlayerFollowJump()
    {
        if (!objectToFollowTransform) yield break;
        
        currentFollowingTime = 0f;
        while (currentFollowingTime < followingTime)
        {
            Vector3 itemPos = transform.position;
            objectToFollowTransform.position =
                new Vector3(itemPos.x, itemPos.y, lockedZ);
            
            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, lockedZ);
            objectToFollowTransform.position = Vector3.Lerp(objectToFollowTransform.position, targetPos, Time.deltaTime * followSpeed);

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
