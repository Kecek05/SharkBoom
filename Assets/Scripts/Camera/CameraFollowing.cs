
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [SerializeField] private CameraManager cameraManager;

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
        ResetCamAfterFollow();
    }

    public void SetTarget(Transform itemLaunched)
    {
        if(itemLaunched == null) return;

        cameraManager.CinemachineCamera.Target.TrackingTarget = itemLaunched;
        cameraManager.CinemachineFollowComponent.FollowOffset.z = cameraManager.CameraObjectToFollow.position.z;

        //cameraManager.CinemachineCamera.FollowOffset = cinemachineFollowCamera.FollowOffset;
        //cinemachineFollowCamera.FollowOffset.z = cameraManager.CameraObjectToFollow.position.z;
    }

    public void ResetCamAfterFollow()
    {
      //  cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow;
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
