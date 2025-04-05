
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private float timeToAwait = 3f;
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
        if(itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = itemLaunched;
        // the problem is that we cant modify the cinemachine camera position

    }

    private IEnumerator ResetCam()
    {
        yield return new WaitForSeconds(timeToAwait);
        ResetCamAfterFollow();
        resetCam = null;
    }

    public void ResetCamAfterFollow()
    {
        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow;
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
