using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerStateMachine
{
    private IState currentState;
    public IState CurrentState => currentState;
    
    private readonly Dictionary<PlayerState, IState> stateMap = new();

    //refs to state objects
    /*private MyTurnStartedState myTurnStartedState;
    private IdleMyTurnState idleMyTurnState;
    private DraggingJump draggingJump;
    private DraggingItem draggingItem;
    private DragReleaseJump dragReleaseJump;
    private DragReleaseItem dragReleaseItem;
    private MyTurnEndedState myTurnEndedState;
    private IdleEnemyTurnState idleEnemyTurnState;
    private PlayerWatchingState playerWatchingState;
    private PlayerGameOverState playerGameOverState;*/
    
    //private PlayerThrower playerThrower;
    public event Action<PlayerState> OnStateChanged;


    public PlayerStateMachine(PlayerThrower player, PlayerDragController playerDragController, PlayerInventory playerInventory, bool isOwner)
    {
        // Build all your state instances
        Register(new MyTurnStartedState(player));
        Register(new IdleMyTurnState(player, playerDragController, playerInventory));
        Register(new DraggingJump(player, playerDragController));
        Register(new DraggingItem(player, playerDragController));
        Register(new DragReleaseJump());
        Register(new DragReleaseItem());
        Register(new MyTurnEndedState(player, isOwner));
        Register(new IdleEnemyTurnState());
        Register(new PlayerWatchingState());
        Register(new PlayerGameOverState());
        //playerThrower = player;
        //our builder
        /*this.myTurnStartedState = new MyTurnStartedState(player);
        this.idleMyTurnState = new IdleMyTurnState(player, playerDragController, playerInventory);
        this.draggingJump = new DraggingJump(player, playerDragController);
        this.draggingItem = new DraggingItem(player, playerDragController);
        this.dragReleaseJump = new DragReleaseJump();
        this.dragReleaseItem = new DragReleaseItem();
        this.myTurnEndedState = new MyTurnEndedState(player);
        this.idleEnemyTurnState = new IdleEnemyTurnState();
        this.playerWatchingState = new PlayerWatchingState();
        this.playerGameOverState = new PlayerGameOverState();*/
    }
    
    private void Register(IState state)
    {
        stateMap[state.State] = state;
    }

    public void Initialize(PlayerState newState)
    {
        IState startingState = GetIStateFromPlayerState(newState);
        
        currentState = startingState;
        currentState.Enter();

        OnStateChanged?.Invoke(currentState.State);
    }
        
    /// <summary>
    /// Called only from PLAYER THROWER to sync states.
    /// </summary>
    /// <param name="nextState"></param>
    public void ChangeStateWithPlayerState(PlayerState nextState)
    {
        DoChangeState(GetIStateFromPlayerState(nextState));
    }

    /// <summary>
    /// Do the actual state change.
    /// </summary>
    /// <param name="nextState"></param>
    private void DoChangeState(IState nextState)
    {
        currentState.Exit();
        Debug.Log($"Old State: {currentState} | Changing to: {nextState}");

        currentState = nextState;
        currentState.Enter();

        OnStateChanged?.Invoke(currentState.State);

    }
    
    /// <summary>
    /// Get the IState from the PlayerState enum.
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns>Returns the IState, Null if not found</returns>
    private IState GetIStateFromPlayerState(PlayerState playerState)
    {
        if (stateMap.TryGetValue(playerState, out var state))
        {
            return state;
        }
        else
        {
            Debug.LogWarning($"No state found for PlayerState: {playerState}");
            return null;
        }
    }

    /*public void Execute()
    {
        if(currentState != null)
        {
            currentState.Execute();
        }
    }*/
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
