using System;

public abstract class BaseItemThrowableActivable : BaseItemThrowable
{
    public event Action OnItemActivated;
    protected bool itemActivated = false;
    protected bool itemCanBeActivated = true;

    protected void OnEnable()
    {
        itemActivated = false;
        itemCanBeActivated = true;
    }
    
    protected override void CollisionController_OnCollidedWithPlayer(PlayerThrower playerObject)
    {
        //Dont allow to activate the item if collided with player
        itemCanBeActivated = false;
    }

    public void TryActivate()
    {
        if (itemActivated) return;
        
        if(!itemCanBeActivated) return;
        
        itemActivated = true;
        ActivateItem();
        OnItemActivated?.Invoke();
    }


    protected abstract void ActivateItem();

}
