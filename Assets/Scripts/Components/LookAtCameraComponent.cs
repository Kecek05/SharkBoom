using UnityEngine;

public class LookAtCameraComponent : MonoBehaviour
{

    [SerializeField] private Mode mode;
    [SerializeField] private Transform canvasTransform;
    private Camera mainCamera;

    private enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate() // we put in late update for performance reasons
    {
            switch (mode)
            {
                case Mode.LookAt:
                    canvasTransform.LookAt(mainCamera.transform);
                    break;
                case Mode.LookAtInverted:
                    Vector3 dirFromCamera = canvasTransform.position - mainCamera.transform.position;
                    canvasTransform.LookAt(canvasTransform.position + dirFromCamera);
                    break;
                case Mode.CameraForward:
                    canvasTransform.forward = mainCamera.transform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    canvasTransform.forward = -mainCamera.transform.forward;
                    break;
            }
    }
}
