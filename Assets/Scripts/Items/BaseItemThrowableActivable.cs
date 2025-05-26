using UnityEngine;

public class BaseItemThrowableActivable : BaseItemThrowable
{

    protected bool itemActivated = false;

    protected void OnEnable()
    {
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
