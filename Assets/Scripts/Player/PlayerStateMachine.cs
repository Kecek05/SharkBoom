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
    public PlayerGameOverState playerGameOverState;

    public event Action<PlayerState> OnStateChanged;


    public PlayerStateMachine(PlayerThrower player)
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
        this.playerGameOverState = new PlayerGameOverState();
    }

    public void Initialize(IState startingState)
    {
        currentState = startingState;
        currentState.Enter();

        OnStateChanged?.Invoke(startingState.State);
    }

    public void TransitionTo(IState nextState)
    {
        currentState.Exit();
        Debug.Log($"Old State: {currentState} | Changing to: {nextState}");


        currentState = nextState;
        nextState.Enter();

        OnStateChanged?.Invoke(nextState.State);

    }

    public void Execute()
    {
        if(currentState != null)
        {
            currentState.Execute();
        }
    }
}

public enum PlayerState
{
    MyTurnStarted,
    IdleMyTurn,
    DraggingJump,
    DraggingItem,
    DragReleaseJump,
    DragReleaseItem,
    MyTurnEnded,
    IdleEnemyTurn,
    PlayerWatching,
    PlayerGameOver
}
