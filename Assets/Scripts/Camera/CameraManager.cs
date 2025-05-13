using System;
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

    private BaseTurnManager turnManager;
    private BasePlayersPublicInfoManager publicInfoManager;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraMovement CameraMovement => cameraMovement;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public Camera CameraMain => cameraMain;


    public void InitializeOwner()
    {
        if (!IsOwner) return;

        if (cameraObjectToFollow == null)
            cameraObjectToFollow = ServiceLocator.Get<CameraObjectToFollow>().transform;

        if (cinemachineCamera == null)
        {
            cinemachineCamera = ServiceLocator.Get<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = cameraObjectToFollow;
            cameraMain = ServiceLocator.Get<Camera>();
        }

        turnManager = ServiceLocator.Get<BaseTurnManager>();
        publicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();

        cameraMovement.InitializeOwner();
        cameraZoom.InitializeOwner();
        cameraFollowing.InitializeOwner();
        turnManager.CurrentPlayableState.OnValueChanged += HandleOnPlayableStateChanged;
    }

    private void HandleOnPlayableStateChanged(PlayableState previousValue, PlayableState newValue)
    {
        enemyObj = publicInfoManager.GetOtherPlayerByMyPlayableState(turnManager.LocalPlayableState);
        playerObj = publicInfoManager.GetPlayerObjectByPlayableState(turnManager.LocalPlayableState);

        if (newValue == player.ThisPlayableState.Value)
        {
            CameraReposOnPlayer();
        }
        else
        {
            CameraReposOnEnemy();
        }
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState playerState)
    {
        switch (playerState)
        {
            default:
                CameraMove();
                break;
            case PlayerState.IdleEnemyTurn:
            case PlayerState.IdleMyTurn:
                CameraMove();
                break;
            case PlayerState.DraggingJump:
            case PlayerState.DraggingItem:
                CameraDragging();
                break;
            case PlayerState.DragReleaseJump:
            case PlayerState.DragReleaseItem:
            case PlayerState.PlayerWatching:
                CameraFollowing();
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
    private void CameraDragging() => SetCameraModules(false, true, false);
    private void CameraFollowing() => SetCameraModules(false, false, true);

    private void CameraReposOnPlayer()
    {
        Debug.Log("CAMERA TEST - Repos on player");
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
    }

    private void CameraReposOnEnemy()
    {
        Debug.Log("CAMERA TEST - Repos on ENEMY");
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(enemyObj.transform, false, 3f); // change for enemy reference
    }

    private void CameraTurnOff()
    {
        Debug.Log("CAMERA TEST - TURN OFF");
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
        SetCameraModules(false, false, false);
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();
    }
}
