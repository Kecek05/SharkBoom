using System.Collections;
using System.Security.Cryptography;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{

    private CinemachineFollow cinemachineFollowCamera;
    private Coroutine followingCourotine;

    
    public void SetTheValuesOfCinemachine(CinemachineFollow _cinemachineFollowCamera)
    {
        cinemachineFollowCamera = _cinemachineFollowCamera;
        FollowingStarted();
    }

    private void FollowingStarted()
    {
        if (followingCourotine == null)
        {
            followingCourotine = StartCoroutine(FollowingCourotine());
        }
    }

    private IEnumerator FollowingCourotine()
    {
        while (cinemachineFollowCamera != null)
        {
            cinemachineFollowCamera.FollowOffset.z = CameraManager.Instance.CameraObjectToFollow.position.z;
            yield return null;
        }
        FollowingEnded();
    }

    private void FollowingEnded()
    {
        if (followingCourotine != null)
        {
            StopCoroutine(followingCourotine);
            followingCourotine = null;
            cinemachineFollowCamera = null;
        }
    }
}
