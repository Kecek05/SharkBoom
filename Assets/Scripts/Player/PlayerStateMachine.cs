using System;


[Serializable]
public class PlayerStateMachine
{
    private IState currentState;
    public IState CurrentState => currentState;

    //refs to state objects
    public IdleState idleState;
    public DeadState deadState;

    public event Action<IState> OnStateChanged;


    public PlayerStateMachine()
    {
        //our builder
        this.idleState = new IdleState();
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
