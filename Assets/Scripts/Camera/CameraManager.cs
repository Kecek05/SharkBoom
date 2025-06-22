using System;
using System.Collections;
using System.Collections.Generic;
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
    private BaseTimerManager timerManager;
    
    /// <summary>
    /// Used to trigger just once the OnValueChanged, the camera must follow based on Item Callbacks and not states
    /// </summary>
    private bool firstPlayableStateChangedTrigger = false; 
    
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public Camera CameraMain => cameraMain;

    public override void OnNetworkSpawn()
    {
        cameraGlobalFollow = ServiceLocator.Get<CameraGlobalFollow>();
        publicInfoManager = ServiceLocator.Get<BasePlayersPublicInfoManager>();
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        timerManager = ServiceLocator.Get<BaseTimerManager>();
    }

    public void InitializeOwner()
    {

        if (!cameraObjectToFollow)
            cameraObjectToFollow = ServiceLocator.Get<CameraObjectToFollow>().transform;

        if (!cinemachineCamera)
        {
            cinemachineCamera = ServiceLocator.Get<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = cameraObjectToFollow;
            cameraMain = ServiceLocator.Get<Camera>();
        }

        cameraMovement.InitializeOwner();
        cameraZoom.InitializeOwner();
    }

    public void CameraGoToActivePlayer()
    {
        if(!IsOwner) return;

        // Debug.Log($"CAMERA GO TO ACTIVE PLAYER - {playerThrower.name} - {playerThrower.PlayerStateMachine.CurrentPlayerState} - Is Owner: {IsOwner}");
        if (playerThrower.PlayerStateMachine.CurrentPlayerState == PlayerState.MyTurnStarted || playerThrower.PlayerStateMachine.CurrentPlayerState == PlayerState.IdleMyTurn)
        {
            CameraGoToPlayer(playerThrower.gameObject);
        } 
        else if (playerThrower.PlayerStateMachine.CurrentPlayerState == PlayerState.IdleEnemyTurn || playerThrower.PlayerStateMachine.CurrentPlayerState == PlayerState.MyTurnEnded)
        {
            foreach (PlayerThrower playerThrower in publicInfoManager.GetAllPlayerThrowers().Values)
            {
                if (this.playerThrower != playerThrower) 
                {
                    //Not this object
                    CameraGoToPlayer(playerThrower.gameObject);
                    return;
                }
            }
        }
    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState playerState)
    {
        if(!IsOwner) return;
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
            if(playerThrower.PlayerStateMachine == null) return;
            
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
        
    }
}
