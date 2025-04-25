using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{

    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private PlayerThrower player;
    
    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public GameObject PlayerObj => playerObj;
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

        BasePlayersPublicInfoManager playersPublicInfoManager = ServiceLocator.Get<PlayersPublicInfoManager>();

        if (CheckPlayer1())
        {
            playerObj = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player1Played) 
                ?? playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player1Playing);
            enemyObj = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player2Played)
                ?? playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player2Playing);
        }
        else
        {
            playerObj = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player2Played)
                ?? playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player2Playing);
            enemyObj = playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player1Played)
                ?? playersPublicInfoManager.GetPlayerObjectByPlayableState(PlayableState.Player1Playing);
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
        SetCameraModules(false, false, false);
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
    }

    private bool CheckPlayer1()
    {
        return player.ThisPlayableState.Value == PlayableState.Player1Playing ||
               player.ThisPlayableState.Value == PlayableState.Player1Played;
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();
    }
}
