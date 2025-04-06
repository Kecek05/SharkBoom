
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollowing : NetworkBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private float timeToAwait = 3f;

    private Transform itemLaunched;
    private Vector3 cameraObjectToFollowPos;
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
        lastCameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position;
        SetTarget(itemLaunched);
    }

    private void BaseItemThrowable_OnItemFinishedAction()
    {
        if (resetCam == null)
        {
            resetCam = StartCoroutine(ResetCam());
        }
    }

    public void SetTarget(Transform itemLaunched)
    {
        if (itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow;
        this.itemLaunched = itemLaunched;

        if (followObject != null)
        {
            StopCoroutine(followObject);
        }

        followObject = StartCoroutine(FollowObject());

    }

    private IEnumerator FollowObject()
    {
        while (itemLaunched != null)
        {
            cameraObjectToFollowPos = cameraManager.CameraObjectToFollow.position;
            cameraObjectToFollowPos.x = itemLaunched.position.x;
            cameraObjectToFollowPos.y = itemLaunched.position.y;

            cameraManager.CameraObjectToFollow.position = cameraObjectToFollowPos;
            yield return null;
        }

        followObject = null;
    }

    private IEnumerator ResetCam()
    {
        yield return new WaitForSeconds(timeToAwait);
        ResetCamAfterFollow();
        resetCam = null;
    }

    public void ResetCamAfterFollow()
    {
       // cameraManager.CameraObjectToFollow.position = lastCameraObjectToFollowPos;
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
