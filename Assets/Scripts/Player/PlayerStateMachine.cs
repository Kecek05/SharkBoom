using System;
using UnityEngine;


[Serializable]
public class PlayerStateMachine
{
    private IState currentState;
    public IState CurrentState => currentState;

    //refs to state objects
    public MyTurnStartedState myTurnStartedState;
    public IdleMyTurnState idleMyTurnState;
    public DraggingJump draggingJump;
    public DraggingItem draggingItem;
    public DragReleaseJump dragReleaseJump;
    public DragReleaseItem dragReleaseItem;
    public MyTurnEndedState myTurnEndedState;
    public IdleEnemyTurnState idleEnemyTurnState;
    public PlayerWatchingState playerWatchingState;
    public DeadState deadState;

    public event Action<IState> OnStateChanged;


    public PlayerStateMachine(Player player)
    {
        //our builder
        this.myTurnStartedState = new MyTurnStartedState(player);
        this.idleMyTurnState = new IdleMyTurnState(player);
        this.draggingJump = new DraggingJump(player);
        this.draggingItem = new DraggingItem(player);
        this.dragReleaseJump = new DragReleaseJump(player);
        this.dragReleaseItem = new DragReleaseItem(player);
        this.myTurnEndedState = new MyTurnEndedState(player);
        this.idleEnemyTurnState = new IdleEnemyTurnState();
        this.playerWatchingState = new PlayerWatchingState();
        this.deadState = new DeadState();
    }

    public void Initialize(IState startingState)
    {
        currentState = startingState;
        currentState.Enter();

        OnStateChanged?.Invoke(startingState);
    }

    public void TransitionTo(IState nextState)
    {
        currentState.Exit();
        Debug.Log($"Old State: {currentState} | Changing to: {nextState}");


        currentState = nextState;
        nextState.Enter();

        OnStateChanged?.Invoke(nextState);

    }

    public void Execute()
    {
        if(currentState != null)
        {
            currentState.Execute();
        }
    }
}
