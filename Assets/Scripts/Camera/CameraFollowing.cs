
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CameraManager cameraManager;

    [Header("Settings")]
    [SerializeField] private float cameraZPosOnFollowing = -12f;

    private WaitForSeconds waitTimeToStopFollowing = new(3f);
    private Transform itemLaunched;
    private Vector3 lastCameraObjectToFollowPos;

    private Coroutine followObject;
    private Coroutine resetCam;



    public void InitializeOwner()
    {
        if(!IsOwner) return;

        BaseItemThrowable.OnItemReleasedAction += HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemFinishedAction += HandleOnItemFinishedAction;
    }



    private void HandleOnItemReleasedAction(Transform itemLaunched)
    {
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position; // store current position of the camera before the item is launched
        SetTarget(itemLaunched, true);
    }

    private void HandleOnItemFinishedAction()
    {
        if (resetCam == null)
            resetCam = StartCoroutine(ResetCam());
    }


    public void SetTarget(Transform itemLaunched, bool stopOnNull, float duration = 5f)
    {
        if (itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        this.itemLaunched = itemLaunched;

        if (followObject != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObject);
        }

        followObject = StartCoroutine(FollowObjectCoroutine(duration, stopOnNull)); 
    }


    private IEnumerator FollowObjectCoroutine(float duration, bool stopOnNull = true)    
    {
        float elapsed = 0f;

        if (stopOnNull)
        {
            while (itemLaunched != null) // while the itemLaunched is not destroyed
            {
                cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y, cameraZPosOnFollowing);
                yield return null;
            }
            followObject = null;
        }
        else
        {
            while (elapsed < duration)
            {
                if (itemLaunched != null)
                {
                    cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y, cameraZPosOnFollowing);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            followObject = null;
        }
    }


    private IEnumerator ResetCam()
    {
        yield return waitTimeToStopFollowing;
        cameraManager.CameraObjectToFollow.position = lastCameraObjectToFollowPos;
        resetCam = null;
    }


    public void UnInitializeOwner()
    {
        if (!IsOwner) return;
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemFinishedAction -= HandleOnItemFinishedAction;
    }

}
