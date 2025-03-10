using Sortify;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseItemThrowableActiveable : BaseItemThrowable
{

    [BetterHeader("Base Activeable References", 11)]
    [SerializeField] protected InputReader inputReader;

    private bool itemActivated = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        itemActivated = false;
    }

    public void TryActivate()
    {
        if (itemActivated || !isServerObject) return; //only server activates the item
        ActivateItem();
        itemActivated = true;
    }


    protected abstract void ActivateItem();
    
}
