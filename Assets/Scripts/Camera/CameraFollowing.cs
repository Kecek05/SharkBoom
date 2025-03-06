using System.Security.Cryptography;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{

    public void SetTheValuesOfCinemachine(CinemachineFollow cinemachineFollowCamera)
    {

        if (cinemachineFollowCamera != null)
        {
            cinemachineFollowCamera.FollowOffset.z = CameraManager.Instance.CameraObjectToFollow.position.z;
        }
        else
        {
            Debug.LogError("CinemachineFollow não encontrado nos filhos de ");
        }
    }

    /*public void SetCameraFollowingObject(Transform _cameraTarget)
    {
        cameraObjectFollowPos = CameraManager.Instance.CameraObjectToFollow.position;
        cameraObjectFollowPos.x += _cameraTarget.position.x;
        cameraObjectFollowPos.y += _cameraTarget.position.y;
        cameraTarget = _cameraTarget;

        Debug.Log(cameraObjectFollowPos.x + ", " + cameraObjectFollowPos.y);
        CameraManager.Instance.CameraObjectToFollow.position = Vector3.MoveTowards(CameraManager.Instance.CameraObjectToFollow.position, _cameraTarget.transform.position, 100f * Time.deltaTime);
        Debug.Log(_cameraTarget + " É o alvo da camera");
    }

    private void Update()
    {
        if(cameraTarget == null)
        {
            return;
        }
        SetCameraFollowingObject(cameraTarget);
    }
    

    public void ResetCameraObject()
    {
        CameraManager.Instance.CinemachineCamera.Follow = CameraManager.Instance.CameraObjectToFollow;
    } */
}
