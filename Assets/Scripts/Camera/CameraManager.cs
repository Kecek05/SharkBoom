using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [SerializeField] private Transform cameraObjectToFollow;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;

    public static CameraManager Instance { get; private set; }
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;

    public CameraFollowing CameraFollowing => cameraFollowing;
    public CameraState GetCameraState => cameraState;

    public enum CameraState // Enum for all camera states
    {
        Move,
        Zoom,
        Dragging,
        Default,
        Following
    }

    [SerializeField] private CameraState cameraState;

    private void Awake() // Singleton
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
        cameraMovement.enabled = false; // We reset all camera Behaviours to false and enable them based on the state
        cameraZoom.enabled = false;
        cameraFollowing.enabled = false;

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
            case CameraState.Following:
                Following();
                break;
        }
    }

    public void SetCameraState(CameraState newState) // Function to update the camera state, parameter for set new state from other scripts
    {
        cameraState = newState;
        CameraManagerState();
    }

    private void Move()
    {
        cameraMovement.enabled = true;
        CameraZoom.enabled = false;
        cameraFollowing.enabled = false;
    }

    private void Zoom()
    {
        cameraZoom.enabled = true;
        cameraMovement.enabled = false;
        cameraFollowing.enabled = false;
    }

    private void Dragging()
    {
        cameraZoom.enabled = true;
        cameraMovement.enabled = false;
        cameraFollowing.enabled = false;
    }

    private void CameraReset()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
        cameraFollowing.enabled = true;
    }

    private void Following()
    {
        cameraFollowing.enabled = true;
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
    }
}
