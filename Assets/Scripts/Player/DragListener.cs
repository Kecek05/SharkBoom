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
            if(this is IInitializeOnwer initializeOnwer)
                this.initializeOnwer = initializeOnwer;
        }

        if (detectDragChange == null)
        {
            if(this is IDetectDragChange detectDragChange)
                this.detectDragChange = detectDragChange;
        }

        if (detectDragRelease == null)
        {
            if(this is IDetectDragRelease detectDragRelease)
                this.detectDragRelease = detectDragRelease;
        }

        if (detectEndedTurn == null)
        {
            if(this is IDetectEndedTurn detectEndedTurn)
                this.detectEndedTurn = detectEndedTurn;
        }

        if (detectDragCancelable == null)
        {
            if(this is IDetectDragCancelable detectDragCancelable)
                this.detectDragCancelable = detectDragCancelable;
        }

        if (detectDragStart == null)
        {
            if(this is IDetectDragStart detectDragStart)
                this.detectDragStart = detectDragStart;
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
