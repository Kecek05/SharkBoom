
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{

    private Action OnComplete;

    [Header("References")]
    [SerializeField] private CameraManager cameraManager;

    [Header("Settings")]
    [SerializeField] private float cameraZPosOnFollowing = -12f;
    [SerializeField] private float followYOffsetForPlayer = 2.5f;
    private WaitForSeconds waitTimeToStopFollowing = new(3f);
    private Transform itemLaunched;
    private Vector3 lastCameraObjectToFollowPos;

    private Coroutine followObject;
    private Coroutine resetCam;



    public void InitializeOwner()
    {
        if(!IsOwner) return;

        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
    }



    private void HandleOnItemReleasedAction(Transform itemLaunched)
    {
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position; // store current position of the camera before the item is launched
        SetTarget(itemLaunched, true);
    }


    public void SetTarget(Transform itemLaunched, bool stopOnNull, float duration = 5f, Action onComplete = null)
    {
        if (itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        this.itemLaunched = itemLaunched;
        this.OnComplete = onComplete;

        if (followObject != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObject);
        }

        followObject = StartCoroutine(FollowObjectCoroutine(duration, stopOnNull)); 
    }


    private IEnumerator FollowObjectCoroutine(float duration, bool stopOnNull = true)    
    {
        float timer = 0f;

        if (stopOnNull)
        {
            // used for items that will be destroyed
            while (itemLaunched != null) // while the itemLaunched is not destroyed
            {
                cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y, cameraZPosOnFollowing);
                yield return null;
            }
            followObject = null;
            OnComplete?.Invoke();
        }
        else
        {
            // used for player or other item that will not be destroyed
            while (timer < duration)
            {
                if (itemLaunched != null)
                {
                    cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y + followYOffsetForPlayer, cameraZPosOnFollowing);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            followObject = null;
            OnComplete?.Invoke();
        }
    }


    public void UnInitializeOwner()
    {
        if (!IsOwner) return;
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
    }

}
