using System;
using System.Threading.Tasks;
using UnityEngine;




public class MyTurnStartedState : IState
{

    //My Turn started, set up only
    private Player player;

    public MyTurnStartedState(Player player)
    {
        //our builder
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Entering My Turn Started State");

        MyTurnStartedCallback();
    }

    private async void MyTurnStartedCallback()
    {
        await Task.Delay(2000);
        player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleMyTurnState);
    }

    public void Execute()
    {
        Debug.Log("Executing My Turn Started State");
    }

    public void Exit()
    {
        Debug.Log("Exiting My Turn Started State");
    }

}

public class IdleMyTurnState : IState
{

    //Idle in my turn
    // Can Move Camera, Choose items and drag
    //Change to Dragging if start dragging

    private Player player;

    public IdleMyTurnState(Player player)
    {
        //our builder
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Entering Idle State");

        player.PlayerDragController.OnDragStart += PlayerDragController_OnDragStart;

        //Set Can Move Camera
    }

    private void PlayerDragController_OnDragStart()
    {
        if (player.PlayerInventory.SelectedItemInventoryIndex == 0)
        {
            player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.draggingJump);
        } else
        {
            player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.draggingItem);
        }
    }

    public void Execute()
    {
        Debug.Log("Executing Idle State");
    }

    public void Exit()
    {
        player.PlayerDragController.OnDragStart -= PlayerDragController_OnDragStart;

        Debug.Log("Exiting Idle State");
    }
}

public class DraggingJump : IState
{
    //Started Dragging the jump
    //Cant Move Camera, Cant Choose items, Only Drag
    //Change to Release Jump if release the jump

    private Player player;


    public DraggingJump(Player player) {
        //our builder
        this.player = player;
    }
    public void Enter()
    {
        Debug.Log("Entering Dragging Jump State");
        //Set Cant move camera
        player.PlayerDragController.OnDragRelease += PlayerDragController_OnDragRelease;
    }

    private void PlayerDragController_OnDragRelease()
    {
        player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.dragReleaseJump);
    }

    public void Execute()
    {
        Debug.Log("Executing Dragging Jump State");
    }

    public void Exit()
    {
        player.PlayerDragController.OnDragRelease -= PlayerDragController_OnDragRelease;

        Debug.Log("Exiting Dragging Jump State");
    }

}

public class DraggingItem : IState
{
    //Started Dragging the item
    //Cant Move Camera, Cant Choose items, Only Drag
    //Change to Release Item if release the item

    private Player player;

    public DraggingItem(Player player)
    {
        //our builder
        this.player = player;
    }
    public void Enter()
    {
        Debug.Log("Entering Dragging Item State");

        player.PlayerDragController.OnDragRelease += PlayerDragController_OnDragRelease;
        //Set Cant move camera

    }

    private void PlayerDragController_OnDragRelease()
    {
        player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.dragReleaseItem);
    }

    public void Execute()
    {
        Debug.Log("Executing Dragging Item State");
    }

    public void Exit()
    {
        player.PlayerDragController.OnDragRelease -= PlayerDragController_OnDragRelease;

        Debug.Log("Exiting Dragging Item State");
    }

}

public class DragReleaseJump : IState
{
    //Released the jump
    //Cant Move Camera, Cant Choose items, Cant Drag, Camera following the action
    //Change to the IdleMyTurn after the item Callback

    private Player player;

    public DragReleaseJump(Player player)
    {
        //our builder
        this.player = player;
    }
    public void Enter()
    {
        Debug.Log("Entering Drag Release Jump State");

        //Set Camera cant move

        // Change to idle on callback

    }

    public void Execute()
    {
        Debug.Log("Executing Drag Release Jump State");
    }
    public void Exit()
    {
        Debug.Log("Exiting Drag Release Jump State");
    }
}

public class DragReleaseItem : IState
{
    //Released the item
    //Cant Move Camera, Cant Choose items, Cant Drag, Camera following the action
    //Change to the MyTurnEnded after the item Callback

    private Player player;

    public DragReleaseItem(Player player)
    {
        //our builder
        this.player = player;
    }
    public void Enter()
    {
        //Set Camera cant move
        Debug.Log("Entering Drag Release Item State");

        // Change to my turn ended on callback
    }

    public void Execute()
    {
        Debug.Log("Executing Drag Release Item State");
    }
    public void Exit()
    {
        Debug.Log("Exiting Drag Release Item State");
    }
}

public class MyTurnEndedState : IState
{
    //My Turn ended, next is enemy turn

    private Player player;

    public MyTurnEndedState(Player player)
    {
        //our builder
        this.player = player;
    }
    public void Enter()
    {
        Debug.Log("Entering My Turn End State");

        GameFlowManager.Instance.PlayerPlayedServerRpc(GameFlowManager.Instance.LocalplayableState);

        MyTurnEndedCallback();

    }

    private async void MyTurnEndedCallback()
    {
        await Task.Delay(2000);
        player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleEnemyTurnState);
    }
    public void Execute()
    {
        Debug.Log("Executing My Turn End State");
    }
    public void Exit()
    {
        Debug.Log("Exiting My Turn End State");
    }
}


public class IdleEnemyTurnState : IState
{
    //Idle in enemy turn
    //Can select items, move camera

    public IdleEnemyTurnState()
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

public class PlayerWatchingState : IState
{
    //Player is watching the enemy turn
    //Cant do anything


    public PlayerWatchingState()
    {
        //our builder
    }
    public void Enter()
    {
        Debug.Log("Entering Player Watching State");
    }
    public void Execute()
    {
        Debug.Log("Executing Player Watching State");
    }
    public void Exit()
    {
        Debug.Log("Exiting Player Watching State");
    }
}


public class PlayerGameOverState : IState
{
    //Cant do anything
    public PlayerGameOverState()
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

