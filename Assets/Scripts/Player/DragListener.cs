using Unity.Netcode;

public abstract class DragListener : NetworkBehaviour
{
    private IInitializeOnwer initializeOnwer;
    private IDetectDragChange detectDragChange;
    private IDetectDragRelease detectDragRelease;
    private IDetectEndedTurn detectEndedTurn;
    private IDetectDragCancelable detectDragCancelable;
    private IDetectDragStart detectDragStart;

    public override void OnNetworkSpawn()
    {
        if(initializeOnwer == null)
        {
            if (this.TryGetComponent(out IInitializeOnwer _initializeOnwer))
            {
                initializeOnwer = _initializeOnwer;
            }
        }

        if (detectDragChange == null)
        {
            if (this.TryGetComponent(out IDetectDragChange _detectDragChange))
            {
                detectDragChange = _detectDragChange;
            }
        }

        if (detectDragRelease == null)
        {
            if (this.TryGetComponent(out IDetectDragRelease _detectDragRelease))
            {
                detectDragRelease = _detectDragRelease;
            }
        }

        if (detectEndedTurn == null)
        {
            if (this.TryGetComponent(out IDetectEndedTurn _detectEndedTurn))
            {
                detectEndedTurn = _detectEndedTurn;
            }
        }

        if (detectDragCancelable == null)
        {
            if (this.TryGetComponent(out IDetectDragCancelable _detectDragCancelable))
            {
                detectDragCancelable = _detectDragCancelable;
            }
        }

        if (detectDragStart == null)
        {
            if (this.TryGetComponent(out IDetectDragStart _detectDragStart))
            {
                detectDragStart = _detectDragStart;
            }
        }
    }

    public void InitializeOwner()
    {
        if (!IsOwner) return;


        initializeOnwer?.DoOnInitializeOnwer();

    }

    public void HandleOnPlayerDragControllerDragStart()
    {
        if (!IsOwner) return; //only owner

        detectDragStart?.DoOnDragStart();
    }

    public void HandleOnPlayerDragControllerDragCancelable(bool isCancelable)
    {
        if (!IsOwner) return; //only owner
        detectDragCancelable?.DoOnDragCancelable(isCancelable);
    }

    public void HandleOnPlayerDragControllerDragChange(float forcePercent, float angle)
    {
        if(!IsOwner) return; //only owner

        detectDragChange?.DoOnDragChange(forcePercent, angle);

    }

    public void HandleOnPlayerStateMachineStateChanged(PlayerState newState)
    {
        if (!IsOwner) return; //only owner

        if (newState == PlayerState.DragReleaseItem || newState == PlayerState.DragReleaseJump)
        {
            detectDragRelease?.DoOnDragRelease();

        } else if (newState == PlayerState.MyTurnEnded)
        {

            detectEndedTurn?.DoOnEndedTurn();
        }
    }
}
