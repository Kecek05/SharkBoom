using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{

    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    [SerializeField] private Transform playerTransform;
    
    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public Transform PlayerTransform => playerTransform;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraMovement CameraMovement => cameraMovement;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public Camera CameraMain => cameraMain;


    public void InitializeOwner()
    {
        if(!IsOwner) return;

        cameraObjectToFollow = ServiceLocator.Get<CameraObjectToFollow>().transform;

        if (cinemachineCamera == null)
        {
            cinemachineCamera = ServiceLocator.Get<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = cameraObjectToFollow;
            cameraMain = ServiceLocator.Get<Camera>();
        }

        cameraMovement.InitializeOwner();
        cameraZoom.InitializeOwner();
        cameraFollowing.InitializeOwner();
        
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
                IdleReposOnPlayer();
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
        Debug.Log($"Player State On The Camera: {playerState}");
    }

    private void CameraMove()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
        cameraFollowing.enabled = false;
    }

    private void IdleReposOnPlayer()
    {
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
        cameraFollowing.enabled = true;
        cameraFollowing.SetTarget(playerTransform, false, 3f);
    }

    private void Dragging()
    {
        cameraZoom.enabled = true;
        cameraMovement.enabled = false;
        cameraFollowing.enabled = false; 
    }
    private void Following()
    {
        cameraFollowing.enabled = true;
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
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
        if (!IsOwner) return;

        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();

    }
}
