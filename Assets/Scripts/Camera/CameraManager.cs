using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private PlayerThrower playerThrower;
    private CinemachineCamera cinemachineCamera;
    private Camera cameraMain; // Cache camera main for all scripts that need it
    private Transform cameraObjectToFollow;

    private GameObject playerObject;
    private GameObject enemyObject;

    private GameObject objectToGo;
    
    private BaseTurnManager turnManager;
    private BasePlayersPublicInfoManager publicInfoManager;
    private CameraGlobalFollow cameraGlobalFollow;
    
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
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

        turnManager.CurrentPlayableState.OnValueChanged += HandleOnPlayableStateChanged;
    }

    private void HandleOnPlayableStateChanged(PlayableState previousValue, PlayableState newValue)
    {
        if(previousValue == newValue) return;
        
        if(newValue == PlayableState.Player1Played || newValue == PlayableState.Player2Played) return;
        
        enemyObject = publicInfoManager.GetOtherPlayerByMyPlayableState(turnManager.LocalPlayableState);
        playerObject = publicInfoManager.GetPlayerObjectByPlayableState(turnManager.LocalPlayableState);

        if (turnManager.LocalPlayableState == newValue)
        {
            CameraGoToPlayer(playerObject);
        }
        else
        {
            CameraGoToPlayer(enemyObject);
        }

        // Debug.Log($"CAMERA GO TO - HandleOnPlayableStateChanged - New Playable State: {newValue} - Enemy: {enemyObject.name} - My: {playerObject.name}");
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

    public void HandleOnPlayerHit(bool isJump)
    {
        cameraGlobalFollow.FollowObject(playerThrower.HipsTransform, isJump: isJump);
    }

    /// <summary>
    /// Disable or Active modules of the camera
    /// </summary>
    /// <param name="movement">CameraMovement Script</param>
    /// <param name="zoom">CameraZoom Script</param>
    /// <param name="following">Camera following</param>
    public void SetCameraModules(bool movement, bool zoom)
    {
        cameraMovement.enabled = movement;
        cameraZoom.enabled = zoom;
    }

    private void CameraMove() => SetCameraModules(true, true);

    private void CameraDragging() => SetCameraModules(false, true);
    
    private void CameraFollowing() => SetCameraModules(false, false);
    
    private void CameraGoToPlayer(GameObject player)
    {
        
        SetCameraModules(false, false);
        cameraGlobalFollow.FollowObject(player.transform, 3f, true, onComplete: () =>
        {
            PlayerState currentState = playerThrower.PlayerStateMachine.CurrentState.State;
            if (currentState == PlayerState.IdleMyTurn || currentState == PlayerState.IdleEnemyTurn)
            {
                CameraMove();
            }
        });
    }

    private void CameraTurnOff()
    {
        SetCameraModules(false, false);
    }

    public void UnInitializeOwner()
    {
        cameraMovement.UnInitializeOwner();
        cameraZoom.UnInitializeOwner();
        
        turnManager.CurrentPlayableState.OnValueChanged -= HandleOnPlayableStateChanged;
    }
}
