using Sortify;
using UnityEngine;

public class BaseItemThrowableActivable : BaseItemThrowable
{

    [BetterHeader("Base Activeable References", 11)]
    [SerializeField] protected InputReader inputReader;


    protected bool itemActivated = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        itemActivated = false;
    }

    public void TryActivate()
    {
        if (itemActivated) return;
        ActivateItem();
    }


    protected virtual void ActivateItem()
    {
        itemActivated = true;
        Debug.Log("Item Activated");
    }
    
}
