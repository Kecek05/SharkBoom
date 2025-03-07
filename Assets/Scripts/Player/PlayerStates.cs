using UnityEngine;



public class IdleState : IState
{

    public IdleState()
    {
        //our builder

    }

    public void Enter()
    {
        Debug.Log("Entering Idle State");
    }

    public void Execute()
    {
        Debug.Log("Executing Idle State");
    }

    public void Exit()
    {
        Debug.Log("Exiting Idle State");
    }
}


public class DeadState : IState
{
    public DeadState()
    {
        //our builder
    }
    public void Enter()
    {
        Debug.Log("Entering Dead State");
    }
    public void Execute()
    {
        Debug.Log("Executing Dead State");
    }
    public void Exit()
    {
        Debug.Log("Exiting Dead State");
    }
}


public class DraggingState : IState
{
    public DraggingState()
    {
        //our builder
    }
    public void Enter()
    {
        Debug.Log("Entering Dragging State");
    }
    public void Execute()
    {
        Debug.Log("Executing Dragging State");
    }
    public void Exit()
    {
        Debug.Log("Exiting Dragging State");
    }
}

public class MyTurnState : IState
{
    public MyTurnState()
    {
        //our builder
    }
    public void Enter()
    {
        Debug.Log("Entering My Turn State");
    }
    public void Execute()
    {
        Debug.Log("Executing My Turn State");
    }
    public void Exit()
    {
        Debug.Log("Exiting My Turn State");
    }
}

public class MyTurnEndedState : IState
{
    public MyTurnEndedState()
    {
        //our builder
    }
    public void Enter()
    {
        Debug.Log("Entering My Turn Ended State");
    }
    public void Execute()
    {
        Debug.Log("Executing My Turn Ended State");
    }
    public void Exit()
    {
        Debug.Log("Exiting My Turn Ended State");
    }
}
