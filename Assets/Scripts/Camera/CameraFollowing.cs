using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowing : NetworkBehaviour
{
    [SerializeField] private CameraManager cameraManager;

    private CinemachineFollow cinemachineFollowCamera;
    private Coroutine followingCourotine;

    public override void OnNetworkSpawn()
    {
        BaseItemThrowable.OnItemReleasedAction += BaseItemThrowable_OnItemReleasedAction;
        BaseItemThrowable.OnItemFinishedAction += BaseItemThrowable_OnItemFinishedAction;
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
        cameraManager.CinemachineCamera.Target.TrackingTarget = itemLaunched;
        cinemachineFollowCamera.FollowOffset.z = cameraManager.CameraObjectToFollow.position.z;
    }

    public void ResetCamAfterFollow()
    {
        cameraManager.CinemachineCamera.Target.TrackingTarget = cameraManager.CameraObjectToFollow;
    }


    public override void OnNetworkDespawn()
    {
        BaseItemThrowable.OnItemReleasedAction -= BaseItemThrowable_OnItemReleasedAction;
        BaseItemThrowable.OnItemFinishedAction -= BaseItemThrowable_OnItemFinishedAction;
    }

}
