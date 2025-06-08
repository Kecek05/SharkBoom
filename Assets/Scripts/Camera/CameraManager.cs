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
    [SerializeField] private NetworkObject playerNetworkObject;
    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    private GameObject playerObj;
    private GameObject enemyObj;

    private BaseTurnManager turnManager;
    private BasePlayersPublicInfoManager publicInfoManager;
    private CameraGlobalFollow cameraGlobalFollow;

    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraMovement CameraMovement => cameraMovement;
    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public Camera CameraMain => cameraMain;

    public override void OnNetworkSpawn()
    {
        cameraGlobalFollow = ServiceLocator.Get<CameraGlobalFollow>();
    }

    public void InitializeOwner()
    {
        if (!IsOwner) return;

        if (!cameraObjectToFollow)
            cameraObjectToFollow = ServiceLocator.Get<CameraObjectToFollow>().transform;

        if (!cinemachineCamera)
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
        
        if (turnManager.LocalPlayableState == newValue)
            CameraGoToSelfPlayer();
        else
            CameraGoToEnemyPlayer();
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState playerState)
    {
        switch (playerState)
        {
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

    public void HandleOnPlayerHit()
    {
        //cameraFollowing.HandleOnPlayerHit();
        cameraGlobalFollow.FollowObject(playerNetworkObject);
    }
    
    public void HandleOnItemCallbackAction()
    {
        //cameraFollowing.HandleOnItemCallbackAction();
    }

    /// <summary>
    /// Disable or Active modules of the camera
    /// </summary>
    /// <param name="movement">CameraMovement Script</param>
    /// <param name="zoom">CameraZoom Script</param>
    /// <param name="following">Camera following</param>
    public void SetCameraModules(bool movement, bool zoom, bool following)
    {
        cameraMovement.enabled = movement;
        cameraZoom.enabled = zoom;
        cameraFollowing.enabled = following;
    }

    private void CameraMove() => SetCameraModules(true, true, false);

    private void CameraDragging() => SetCameraModules(false, true, false);
    
    private void CameraFollowing() => SetCameraModules(false, false, true);

    private void CameraGoToSelfPlayer()
    {
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(playerObj.transform, false, 3f, onComplete: () =>
        {
            PlayerState currentState = playerReference.PlayerStateMachine.CurrentState.State;

            if (currentState == PlayerState.IdleMyTurn || currentState == PlayerState.IdleEnemyTurn)
            {
                CameraMove();
            }
        });
    }

    private void CameraGoToEnemyPlayer()
    {
        SetCameraModules(false, false, true);
        cameraFollowing.SetTarget(enemyObj.transform, false, 3f, onComplete: () =>
        {
            PlayerState currentState = playerReference.PlayerStateMachine.CurrentState.State;

            if (currentState == PlayerState.IdleMyTurn || currentState == PlayerState.IdleEnemyTurn)
            {
                CameraMove();
            }
        });
    }

    private void CameraTurnOff()
    {
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
