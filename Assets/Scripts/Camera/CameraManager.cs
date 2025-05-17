using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    [SerializeField] private PlayerThrower playerReference;
    
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

        if (IsMyTurn(newValue))
        {
            CameraReposOnPlayer();
        }
        else if (IsEnemyTurn(newValue))
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
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(playerObj.transform, false, 3f, onComplete: CameraMove);
    }

    private void CameraReposOnEnemy()
    {
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(enemyObj.transform, false, 3f, onComplete: CameraMove);
    }

    private void CameraTurnOff()
    {
        cameraFollowing.SetTarget(playerObj.transform, false, 3f);
        SetCameraModules(false, false, false);
    }

    private bool IsMyTurn(PlayableState state)
    {
        return state == PlayableState.Player1Playing && turnManager.LocalPlayableState == PlayableState.Player1Playing
            || state == PlayableState.Player2Playing && turnManager.LocalPlayableState == PlayableState.Player2Playing;
    }
    private bool IsEnemyTurn(PlayableState state)
    {
        return state == PlayableState.Player1Playing && turnManager.LocalPlayableState == PlayableState.Player2Playing
            || state == PlayableState.Player2Playing && turnManager.LocalPlayableState == PlayableState.Player1Playing;
    }

    public void UnInitializeOwner()
    {
        if (!IsOwner) return;

        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        cameraFollowing.UnInitializeOwner();
    }
}
