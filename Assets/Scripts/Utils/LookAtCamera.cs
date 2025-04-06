using Unity.Netcode;
using UnityEngine;

public class LookAtCamera : NetworkBehaviour
{

    [SerializeField] private Mode mode;
     

    private enum Mode
    {
        LookAt,
        LateAtInverted,
        CameraForward,
        CameraForwardInverted
    }

    private void LateUpdate() // we put in late update for performance reasons
    {
        if(IsOwner)
        {
            switch (mode)
            {
                case Mode.LookAt:
                    transform.LookAt(Camera.main.transform);
                    break;
                case Mode.LateAtInverted:
                    Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                    transform.LookAt(transform.position + dirFromCamera);
                    break;
                case Mode.CameraForward:
                    transform.forward = Camera.main.transform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    transform.forward = -Camera.main.transform.forward;
                    break;
            }
        }
    }
}
