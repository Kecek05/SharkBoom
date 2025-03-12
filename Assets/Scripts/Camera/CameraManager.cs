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

    private Transform cameraObjectToFollow;

    public static CameraManager Instance { get; private set; }
    public Transform CameraObjectToFollow => cameraObjectToFollow;
    public CameraZoom CameraZoom => cameraZoom;

    public CameraFollowing CameraFollowing => cameraFollowing;
    public CameraState GetCameraState => cameraState;

    public enum CameraState // Enum for all camera states
    {
        Move,
        Zoom,
        Dragging,
        Default,
        Following
    }

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
        }
    }

    

    private void PlayerStateMachine_OnStateChanged(IState playerState)
    {
        if(playerState == player.PlayerStateMachine.idleEnemyTurnState)
        {
            CameraMove();
        }
        else if(playerState == player.PlayerStateMachine.myTurnEndedState)
        {
            CameraReset();
        }
        else if (playerState == player.PlayerStateMachine.myTurnStartedState)
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
            CameraMove();
        }
        
        Debug.Log($"Player State: {player.PlayerStateMachine.CurrentState}");
    }

    public void CameraManagerState()
    {
        cameraMovement.enabled = false; // We reset all camera Behaviours to false and enable them based on the state
        cameraZoom.enabled = false;
        cameraFollowing.enabled = false;

        //switch (cameraState)
        //{
        //    case CameraState.Default:
        //        CameraReset();
        //        break;
        //    case CameraState.Move:
        //        Move();
        //        break;
        //    case CameraState.Zoom:
        //        Zoom();
        //        break;
        //    case CameraState.Dragging:
        //        Dragging();
        //        break;
        //    case CameraState.Following:
        //        Following();
        //        break;
        //}

        
    }

    //public void SetCameraState(CameraState newState) // Function to update the camera state, parameter for set new state from other scripts
    //{
    //    cameraState = newState;
    //    CameraManagerState();
    //}

    private void CameraMove()
    {
        if(Input.touchCount == 1)
        {
            cameraMovement.enabled = true;
            cameraZoom.enabled = false;
            cameraFollowing.enabled = false;
            Debug.Log("one input");
        }
        else
        {
            CameraZoom.enabled = true;
            cameraMovement.enabled = false;
            cameraFollowing.enabled = false;
        }
    }

    private void Dragging()
    {
        cameraZoom.enabled = true;
        cameraMovement.enabled = false;
        cameraFollowing.enabled = false;
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
        cameraMovement.enabled = false;
        cameraZoom.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        player.PlayerStateMachine.OnStateChanged -= PlayerStateMachine_OnStateChanged;
    }
}
