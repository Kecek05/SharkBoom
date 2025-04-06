using Unity.Netcode;
using UnityEngine;

public class LookAtCamera : NetworkBehaviour
{

    [SerializeField] private Mode mode;
    [SerializeField] private Transform canvasTransform;
     

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
                    canvasTransform.LookAt(Camera.main.transform);
                    break;
                case Mode.LateAtInverted:
                    Vector3 dirFromCamera = canvasTransform.position - Camera.main.transform.position;
                    canvasTransform.LookAt(canvasTransform.position + dirFromCamera);
                    break;
                case Mode.CameraForward:
                    canvasTransform.forward = Camera.main.transform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    canvasTransform.forward = -Camera.main.transform.forward;
                    break;
            }
        }
    }
}
