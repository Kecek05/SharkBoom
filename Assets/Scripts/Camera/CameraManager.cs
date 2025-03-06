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


    public enum CameraState // Enum for all camera states
    {
        Move,
        Zoom,
        Dragging,
        Default,
        Following
    }

    private CameraState cameraState;

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
    }

    private void Zoom()
    {
        cameraZoom.enabled = true;
    }

    private void Dragging()
    {
        cameraMovement.enabled = false;
        cameraZoom.enabled = true;
    }

    private void CameraReset()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
    }

    private void Following()
    {
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
        cameraFollowing.enabled = true;
        Debug.Log("Camera Following is true");
    }
}
