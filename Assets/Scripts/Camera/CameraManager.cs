using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    [SerializeField] private PlayerThrower player;
    
    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    private GameObject playerObj;
    private GameObject enemyObj;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraMovement CameraMovement => cameraMovement;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public Camera CameraMain => cameraMain;


    public void InitializeOwner()
    {
        if(!IsOwner) return;

        if (cameraObjectToFollow == null)
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

        BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

        if (CheckPlayer1())
        {
            playerObj = GetPlayerObject(ServiceLocator.Get<BaseTurnManager>().LocalPlayableState);
            enemyObj = GetPlayerObject(PlayableState.Player2Playing);
        }

        switch (playerState)
        {
            default:
                CameraMove();
                break;

            case PlayerState.MyTurnStarted:
                ReposOnPlayer();
                break;
            case PlayerState.MyTurnEnded:
                ReposOnEnemy();
                break;

            case PlayerState.IdleEnemyTurn:
            case PlayerState.IdleMyTurn:
                CameraMove();
                break;

            case PlayerState.DraggingJump:
            case PlayerState.DraggingItem:
                Dragging();
                break;

            case PlayerState.DragReleaseJump:
            case PlayerState.DragReleaseItem:
            case PlayerState.PlayerWatching:
                Following();
                break;

            case PlayerState.PlayerGameOver:
                CameraTurnOff();
                break;
        }
    }

    /// <summary>
    /// Disable or Active modules of the camera
    /// </summary>
    /// <param name="movement">CameraMovement Script</param>
    /// <param name="zoom">CameraZoom Script</param>
    /// <param name="following">Camera following</param>
    private void SetCameraModules(bool movement, bool zoom, bool following)
    {
        cameraMovement.enabled = movement;
        cameraZoom.enabled = zoom;
        cameraFollowing.enabled = following;
    }

    private void CameraMove() => SetCameraModules(true, true, false);
    private void Dragging() => SetCameraModules(false, true, false);
    private void Following() => SetCameraModules(false, false, true);

    private void ReposOnPlayer()
    {
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
    }

    private void ReposOnEnemy()
    {
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(enemyObj.transform, false, 3f); // change for enemy reference
    }

    private void CameraTurnOff()
    {
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
        SetCameraModules(false, false, false);
    }

    private bool CheckPlayer1()
    {
        return player.ThisPlayableState.Value == PlayableState.Player1Playing;
    }

    private GameObject GetPlayerObject(PlayableState playingState)
    {
        BasePlayersPublicInfoManager manager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
        return manager.GetPlayerObjectByPlayableState(playingState);
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();
    }
}
