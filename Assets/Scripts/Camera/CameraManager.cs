using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [SerializeField] private Transform cameraObjectToFollow;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;

    public static CameraManager Instance { get; private set; }
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;

    public enum CameraState // Enum for all camera states
    {
        Move,
        Zoom,
        Dragging,
        Default
    }

    private CameraState cameraState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void CameraManagerState()
    {
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;

        switch (cameraState)
        {
            case CameraState.Default:
                CameraReset();
                break;
            case CameraState.Move:
                Move();
                break;
            case CameraState.Zoom:
                Zoom();
                break;
            case CameraState.Dragging:
                Dragging();
                break;
        }
    }

    public void SetCameraState(CameraState newState) // Function to update the camera state
    {
        cameraState = newState;
        CameraManagerState();
    }

    private void Move()
    {
        cameraMovement.enabled = true;
    }

    private void Zoom()
    {
        cameraZoom.enabled = true;
    }

    private void Dragging()
    {
        cameraMovement.enabled = false;

    }

    private void CameraReset()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
    }
}
