using System;

public abstract class BaseItemThrowableActivable : BaseItemThrowable
{
    public event Action OnItemActivated;
    protected bool itemActivated = false;

    protected void OnEnable()
    {
        itemActivated = false;
    }
    
    protected override void CollisionController_OnCollidedWithPlayer(PlayerThrower playerObject)
    {
        //Dont allow to activate the item if collided with player
        itemActivated = true;
    }

    public void TryActivate()
    {
        if (itemActivated) return;
        ActivateItem();
        OnItemActivated?.Invoke();
    }


    protected abstract void ActivateItem();

}
