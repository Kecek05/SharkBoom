using System;

public abstract class BaseItemThrowableActivable : BaseItemThrowable
{
    public event Action OnItemActivated;
    protected bool itemActivated = false;

    protected void OnEnable()
    {
        itemActivated = false;
    }

    public void TryActivate()
    {
        if (itemActivated) return;
        ActivateItem();
        OnItemActivated?.Invoke();
    }


    protected abstract void ActivateItem();

}
