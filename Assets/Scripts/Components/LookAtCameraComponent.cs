using Unity.Netcode;
using UnityEngine;

public class LookAtCameraComponent : NetworkBehaviour
{

    [SerializeField] private Mode mode;
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private CameraManager cameraManager;


    private enum Mode
    {
        LookAt,
        LateAtInverted,
        CameraForward,
        CameraForwardInverted
    }

    private void LateUpdate() // we put in late update for performance reasons
    {
        if (IsOwner)
        {
            switch (mode)
            {
                case Mode.LookAt:
                    canvasTransform.LookAt(cameraManager.CameraMain.transform);
                    break;
                case Mode.LateAtInverted:
                    Vector3 dirFromCamera = canvasTransform.position - cameraManager.CameraMain.transform.position;
                    canvasTransform.LookAt(canvasTransform.position + dirFromCamera);
                    break;
                case Mode.CameraForward:
                    canvasTransform.forward = cameraManager.CameraMain.transform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    canvasTransform.forward = -cameraManager.CameraMain.transform.forward;
                    break;
            }
        }
    }
}
