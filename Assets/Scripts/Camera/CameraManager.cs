using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class CameraManager : NetworkBehaviour
{

    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    


    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraMovement CameraMovement => cameraMovement;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public Camera CameraMain => cameraMain;

    [SerializeField] private CameraState cameraState;

    public void InitializeOwner()
    {
        if(!IsOwner) return;

        cameraObjectToFollow = new GameObject("CameraObjectToFollow").transform;
        cameraObjectToFollow.position = new Vector3(0, 0, 0);

        if (cinemachineCamera == null)
        {
            cinemachineCamera = Object.FindFirstObjectByType<CinemachineCamera>();
            cameraMain = Object.FindFirstObjectByType<Camera>();
            cinemachineCamera.Target.TrackingTarget = cameraObjectToFollow;
        }

        cameraMovement.InitializeOwner();
        cameraZoom.InitializeOwner();
        cameraFollowing.InitializeOwner();

        CameraMove();
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState playerState)
    {
        if(!IsOwner) return;

        switch(playerState)
        {
            default:
                CameraMove();
                break;
            case PlayerState.MyTurnStarted:
                CameraMove();
                break;
            case PlayerState.IdleEnemyTurn:
                CameraMove();
                break;
            case PlayerState.IdleMyTurn:
                CameraMove();
                break;
            case PlayerState.DraggingJump:
                Dragging();
                break;
            case PlayerState.DraggingItem:
                Dragging();
                break;
            case PlayerState.DragReleaseJump:
                Following();
                break;
            case PlayerState.DragReleaseItem:
                Following();
                break;
            case PlayerState.PlayerWatching:
                Following();
                break;
            case PlayerState.MyTurnEnded:
                CameraReset();
                break;
            case PlayerState.PlayerGameOver:
                //turn off camera and focus on the dead player
                CameraTurnOff();
                break;
        }
    }

    private void CameraMove()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
    }

    private void Dragging()
    {
        cameraZoom.enabled = true;
        cameraMovement.enabled = false;
        Debug.Log("Dragging");  
    }
    private void Following()
    {
        cameraFollowing.enabled = true;
    }

    private void CameraReset()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
        cameraFollowing.enabled = true;
    }

    private void CameraTurnOff()
    {
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
        cameraFollowing.enabled = false;
    }

    public void UnInitializeOwner()
    {
        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();
    }
}
