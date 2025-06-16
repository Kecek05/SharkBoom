using System.Threading.Tasks;
using UnityEngine;




public class MyTurnStartedState : IState
{

    //My Turn started, set up only
    private PlayerThrower player;
    private PlayerState state = PlayerState.MyTurnStarted;

    public PlayerState State => state;

    public MyTurnStartedState(PlayerThrower player)
    {
        //our builder
        this.player = player;
    }

    public void Enter()
    {
       //Debug.Log("Entering My Turn Started State");

        MyTurnStartedCallback();
    }

    private async void MyTurnStartedCallback()
    {
        await Task.Delay(2000);
        player.ChangePlayerState(PlayerState.IdleMyTurn);
        /*player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleMyTurnState);
        player.TransitionToMyTurnEndedStateServerRpc();*/
    }

    public void Execute()
    {
        //Debug.Log("Executing My Turn Started State");
    }

    public void Exit()
    {
        //Debug.Log("Exiting My Turn Started State");
    }

}

public class IdleMyTurnState : IState
{

    //Idle in my turn
    // Can Move Camera, Choose items and drag
    //Change to Dragging if start dragging
    private PlayerThrower player;
    private PlayerDragController playerDragController;
    private PlayerInventory playerInventory;
    private PlayerState state = PlayerState.IdleMyTurn;

    public PlayerState State => state;

    public IdleMyTurnState(PlayerThrower player ,PlayerDragController playerDragController, PlayerInventory playerInventory)
    {
        //our builder
        this.player = player;
        this.playerDragController = playerDragController;
        this.playerInventory = playerInventory;
    }

    public void Enter()
    {
        //Debug.Log("Entering Idle State");

        playerDragController.OnDragStart += PlayerDragController_OnDragStart;
        
        //Set Can Move Camera
    }

    private void PlayerDragController_OnDragStart()
    {
        if (playerInventory.SelectedItemID == 0)
        {
            player.ChangePlayerState(PlayerState.DraggingJump);
            /*player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.draggingJump);
            player.TransitionToDraggingJumpServerRpc();*/
        } else
        {
            player.ChangePlayerState(PlayerState.DraggingItem);
            /*player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.draggingItem);
            player.TransitionToDraggingItemServerRpc();*/
        }
    }

    public void Execute()
    {
        //Debug.Log("Executing Idle State");
    }

    public void Exit()
    {
        playerDragController.OnDragStart -= PlayerDragController_OnDragStart;

        //Debug.Log("Exiting Idle State");
    }
}

public class DraggingJump : IState
{
    //Started Dragging the jump
    //Cant Move Camera, Cant Choose items, Only Drag
    //Change to Release Jump if release the jump

    private PlayerThrower player;
    private PlayerDragController playerDragController;
    private PlayerState state = PlayerState.DraggingJump;
    private PlayerInventory playerInventory;
    public PlayerState State => state;

    public DraggingJump(PlayerThrower player, PlayerDragController playerDragController, PlayerInventory playerInventory) {
        //our builder
        this.player = player;
        this.playerDragController = playerDragController;
        this.playerInventory = playerInventory;
    }
    public void Enter()
    {
        //Debug.Log("Entering Dragging Jump State");
        //Set Cant move camera
        Debug.Log($"STEPS SUBSCRIBING TO DRAG JUMP RELEASE - {player.gameObject.name} - {playerDragController.gameObject.name}");
        playerDragController.OnDragRelease += PlayerDragController_OnDragRelease;
        playerInventory.OnItemSelected += PlayerInventory_OnItemSelected;
    }

    private void PlayerInventory_OnItemSelected(int itemID)
    {
        if (itemID != 0)
        {
            //Selected an item that isnt jump, change the state
            player.ChangePlayerState(PlayerState.IdleMyTurn);
        }
    }

    private void PlayerDragController_OnDragRelease()
    {
        player.ChangePlayerState(PlayerState.DragReleaseJump);
        /*player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.dragReleaseJump);
        player.TransitionToDraggingJumpServerRpc();*/
    }

    public void Execute()
    {
       // Debug.Log("Executing Dragging Jump State");
    }

    public void Exit()
    {
        playerDragController.OnDragRelease -= PlayerDragController_OnDragRelease;
        playerInventory.OnItemSelected -= PlayerInventory_OnItemSelected;
        //Debug.Log("Exiting Dragging Jump State");
    }

}

public class DraggingItem : IState
{
    //Started Dragging the item
    //Cant Move Camera, Cant Choose items, Only Drag
    //Change to Release Item if release the item

    private PlayerThrower player;
    private PlayerDragController playerDragController;
    private PlayerState state = PlayerState.DraggingItem;
    private PlayerInventory playerInventory;

    public PlayerState State => state;

    public DraggingItem(PlayerThrower player, PlayerDragController playerDragController, PlayerInventory playerInventory)
    {
        //our builder
        this.player = player;
        this.playerDragController = playerDragController;
        this.playerInventory = playerInventory;
    }
    public void Enter()
    {
        //Debug.Log("Entering Dragging Item State");
        Debug.Log($"STEPS SUBSCRIBING TO DRAG RELEASE - {player.gameObject.name} - {playerDragController.gameObject.name}");
        playerDragController.OnDragRelease += PlayerDragController_OnDragRelease;
        playerInventory.OnItemSelected += PlayerInventory_OnItemSelected;
        //Set Cant move camera

    }

    private void PlayerInventory_OnItemSelected(int itemID)
    {
        if (itemID == 0)
        {
            //Selected Jump
            player.ChangePlayerState(PlayerState.IdleMyTurn);
        }
    }

    private void PlayerDragController_OnDragRelease()
    {
        Debug.Log($"STEPS CLIENT 4 - DRAGGING ITEM TO DRAG RELEASE ITEM - {player.gameObject.name}");
        player.ChangePlayerState(PlayerState.DragReleaseItem);
        //player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.dragReleaseItem);
    }

    public void Execute()
    {
        //Debug.Log("Executing Dragging Item State");
    }

    public void Exit()
    {
        Debug.Log($"STEPS UNSUBSCRIBING TO DRAG RELEASE - {player.gameObject.name} - {playerDragController.gameObject.name}");
        playerDragController.OnDragRelease -= PlayerDragController_OnDragRelease;
        playerInventory.OnItemSelected -= PlayerInventory_OnItemSelected;
        //Debug.Log("Exiting Dragging Item State");
    }

}

public class DragReleaseJump : IState
{
    //Released the jump
    //Cant Move Camera, Cant Choose items, Cant Drag, Camera following the action
    //Change to the IdleMyTurn after the item Callback


    private PlayerState state = PlayerState.DragReleaseJump;

    public PlayerState State => state;

    public DragReleaseJump()
    {
        //our builder

    }
    public void Enter()
    {
        //Debug.Log("Entering Drag Release Jump State");

        //Set Camera cant move

        // Change to idle on callback

    }

    public void Execute()
    {
        //Debug.Log("Executing Drag Release Jump State");
    }
    public void Exit()
    {
        //Debug.Log("Exiting Drag Release Jump State");
    }
}

public class DragReleaseItem : IState
{
    //Released the item
    //Cant Move Camera, Cant Choose items, Cant Drag, Camera following the action
    //Change to the MyTurnEnded after the item Callback

    private PlayerState state = PlayerState.DragReleaseItem;

    public PlayerState State => state;

    public DragReleaseItem()
    {
        //our builder
    }
    public void Enter()
    {
        //Set Camera cant move
        //Debug.Log("Entering Drag Release Item State");

        // Change to my turn ended on callback
    }

    public void Execute()
    {
        //Debug.Log("Executing Drag Release Item State");
    }
    public void Exit()
    {
        //Debug.Log("Exiting Drag Release Item State");
    }
}

public class MyTurnEndedState : IState
{
    //My Turn ended, next is enemy turn

    private PlayerThrower player;
    private BaseTurnManager turnManager;
    private PlayerState state = PlayerState.MyTurnEnded;
    private bool isOwner;
    
    public PlayerState State => state;

    public MyTurnEndedState(PlayerThrower player, bool isOwner)
    {
        //our builder
        this.player = player;
        this.isOwner = isOwner;
    }
    public void Enter()
    {
        //Debug.Log("Entering My Turn End State");
        if (isOwner)
        {
            turnManager = ServiceLocator.Get<BaseTurnManager>();
            turnManager.PlayerPlayed(turnManager.LocalPlayableState);
        }

        MyTurnEndedCallback();
    }
    
    public void ChangeOwnership(bool isOwner)
    {
        this.isOwner = isOwner;
        Debug.Log($"MyTurnEndedState ChangeOwnership: {this.isOwner} - {player.gameObject.name}");
    }

    private async void MyTurnEndedCallback()
    {
        await Task.Delay(2000);
        player.ChangePlayerState(PlayerState.IdleEnemyTurn);
       //player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleEnemyTurnState);
    }
    public void Execute()
    {
        //Debug.Log("Executing My Turn End State");
    }
    public void Exit()
    {
        //Debug.Log("Exiting My Turn End State");
    }
}


public class IdleEnemyTurnState : IState
{
    //Idle in enemy turn
    //Can select items, move camera
    private PlayerState state = PlayerState.IdleEnemyTurn;

    public PlayerState State => state;
    public IdleEnemyTurnState()
    {
        //our builder
    }
    public void Enter()
    {
        //Debug.Log("Entering Idle State");
    }
    public void Execute()
    {
        //Debug.Log("Executing Idle State");
    }
    public void Exit()
    {
       // Debug.Log("Exiting Idle State");
    }
}

public class PlayerWatchingState : IState
{
    //Player is watching the enemy turn
    //Cant do anything
    private PlayerState state = PlayerState.PlayerWatching;

    public PlayerState State => state;

    public PlayerWatchingState()
    {
        //our builder
    }
    public void Enter()
    {
        //Debug.Log("Entering Player Watching State");
    }
    public void Execute()
    {
        //Debug.Log("Executing Player Watching State");
    }
    public void Exit()
    {
        //Debug.Log("Exiting Player Watching State");
    }
}


public class PlayerGameOverState : IState
{
    //Cant do anything
    private PlayerState state = PlayerState.PlayerGameOver;

    public PlayerState State => state;

    public PlayerGameOverState()
    {
        //our builder
    }
    public void Enter()
    {
        //Debug.Log("Entering Dead State");
    }
    public void Execute()
    {
        //Debug.Log("Executing Dead State");
    }
    public void Exit()
    {
        //Debug.Log("Exiting Dead State");
    }
}

