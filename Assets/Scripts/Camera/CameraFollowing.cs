
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CameraManager cameraManager;

    [Header("Settings")]
    [Tooltip("Time to pause camera on item finish position")]
    [SerializeField] private float cameraZPosOnFollowing = -12f;

    private WaitForSeconds waitToStopFollowing = new(3f);
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
        SetTarget(itemLaunched);
    }

    private void HandleOnItemFinishedAction()
    {
        if (resetCam == null)
        {
            resetCam = StartCoroutine(ResetCam()); // when the item finished their action, we start the coroutine to reset the camera
        }
    }

    public void SetTarget(Transform itemLaunched, bool stopOnNull = true, float duration = 5f) // Here we set the target and start the coroutine to follow the object
    {
        if (itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        this.itemLaunched = itemLaunched;

        if (followObject != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObject);
        }

        followObject = StartCoroutine(FollowObject(stopOnNull, duration)); 

    }

    private IEnumerator FollowObject(bool stopOnNull = true, float duration)    
    {
        if(stopOnNull)
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
            float elapsed = 0f;

            if (stopOnNull)
            {
                while (itemLaunched != null)
                {
                    cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y, cameraZPosOnFollowing);
                    yield return null;
                }
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
            }

            followObject = null;
        }
    }


    private IEnumerator ResetCam()
    {
        yield return waitToStopFollowing;
        resetCam = null;
    }


    public void UnInitializeOwner()
    {
        if (!IsOwner) return;
        BaseItemThrowable.OnItemReleasedAction -= HandleOnItemReleasedAction;
        BaseItemThrowable.OnItemFinishedAction -= HandleOnItemFinishedAction;
    }

}
