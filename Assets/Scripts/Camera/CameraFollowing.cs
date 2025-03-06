using System.Security.Cryptography;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{

    private Vector3 cameraObjectFollowPos;

    public void SetCameraFollowingObject(Transform cameraTarget)
    {
        cameraObjectFollowPos = CameraManager.Instance.CameraObjectToFollow.position;
        cameraObjectFollowPos.x += cameraTarget.position.x;
        cameraObjectFollowPos.y += cameraTarget.position.y;

        CameraManager.Instance.CameraObjectToFollow.position = Vector3.MoveTowards(CameraManager.Instance.CameraObjectToFollow.position, cameraTarget.transform.position, Time.deltaTime);
        Debug.Log(cameraTarget + " É o alvo da camera");
    }

    public void ResetCameraObject()
    {
        CameraManager.Instance.CinemachineCamera.Follow = CameraManager.Instance.CameraObjectToFollow;
    }
}
