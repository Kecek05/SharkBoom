
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CameraManager cameraManager;

    [Header("Settings")]
    [Tooltip("Time to pause camera on item finish position")]
    [SerializeField] private float timeToAwait = 3f; 

    private Transform itemLaunched;
    private Vector3 lastCameraObjectToFollowPos;
    private Coroutine followObject;
    private Coroutine resetCam;


    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            BaseItemThrowable.OnItemReleasedAction += BaseItemThrowable_OnItemReleasedAction;
            BaseItemThrowable.OnItemFinishedAction += BaseItemThrowable_OnItemFinishedAction;
        }
    }


    private void BaseItemThrowable_OnItemReleasedAction(Transform itemLaunched)
    {
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position; // store current position of the camera before the item is launched
        SetTarget(itemLaunched);
    }


    public void SetTarget(Transform itemLaunched) // Here we set the target and start the coroutine to follow the object
    {
        if (itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow; // make sure the camera is following the object
        this.itemLaunched = itemLaunched;

        if (followObject != null) // if the coroutine is already running, stop it
        {
            StopCoroutine(followObject);
        }

        followObject = StartCoroutine(FollowObject()); 

    }

    private IEnumerator FollowObject()
    {
        while (itemLaunched != null) // while the itemLaunched is not destroyed
        {
            cameraManager.CameraObjectToFollow.position = new Vector3(itemLaunched.position.x, itemLaunched.position.y, cameraManager.CameraObjectToFollow.position.z); // we set the position of the camera based on the itemLaunched x and y, and we maintain the z position of the camera
            yield return null;
        }

        followObject = null;
    }

    private void BaseItemThrowable_OnItemFinishedAction()
    {
        if (resetCam == null)
        {
            resetCam = StartCoroutine(ResetCam()); // when the item finished their action, we start the coroutine to reset the camera
        }
    }

    private IEnumerator ResetCam()
    {
        yield return new WaitForSeconds(timeToAwait);
        ResetCamAfterFollow();
        resetCam = null;
    }

    public void ResetCamAfterFollow()
    {
       cameraManager.CameraObjectToFollow.position = lastCameraObjectToFollowPos; // we send the camera for last position before the item was launched
    }


    public override void OnNetworkDespawn()
    {
        if(IsOwner)
        {
            BaseItemThrowable.OnItemReleasedAction -= BaseItemThrowable_OnItemReleasedAction;
            BaseItemThrowable.OnItemFinishedAction -= BaseItemThrowable_OnItemFinishedAction;
        }
    }

}
