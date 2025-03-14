using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class CameraManager : NetworkBehaviour
{

    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CameraFollowing cameraFollowing;
    [SerializeField] private Player player;

    private CinemachineCamera cinemachineCamera;
    private Transform cameraObjectToFollow;

    public static CameraManager Instance { get; private set; }
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;
    public CameraFollowing CameraFollowing => cameraFollowing;
    public CameraMovement CameraMovement => cameraMovement;

    [SerializeField] private CameraState cameraState;


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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.PlayerStateMachine.OnStateChanged += PlayerStateMachine_OnStateChanged;
            cameraObjectToFollow = new GameObject("CameraObjectToFollow").transform;
            cameraObjectToFollow.position = new Vector3(0, 0, 0);

            if (cinemachineCamera == null)
            {
                cinemachineCamera = Object.FindFirstObjectByType<CinemachineCamera>();
                cinemachineCamera.Target.TrackingTarget = cameraObjectToFollow;
            }
        }
    }

    private void PlayerStateMachine_OnStateChanged(IState playerState)
    {
        if (playerState == player.PlayerStateMachine.idleEnemyTurnState)
        {
            CameraMove();
        }
        else if (playerState == player.PlayerStateMachine.idleMyTurnState)
        {
            CameraMove();
        }
        else if (playerState == player.PlayerStateMachine.draggingJump || playerState == player.PlayerStateMachine.draggingItem)
        {
            Dragging();
        }
        else if (playerState == player.PlayerStateMachine.dragReleaseJump || playerState == player.PlayerStateMachine.dragReleaseItem)
        {
            Following();
        }
        else if (playerState == player.PlayerStateMachine.deadState)
        {
            CameraReset();
        }
        else if (playerState == player.PlayerStateMachine.playerWatchingState)
        {
            // when enemy realase the item
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
    }

    private void CameraReset()
    {
        cameraMovement.enabled = true;
        cameraZoom.enabled = true;
        cameraFollowing.enabled = true;
    }

    private void Following()
    {
        cameraFollowing.enabled = true;
    }

    public override void OnNetworkDespawn()
    {
        player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
    }
}
