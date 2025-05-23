
using UnityEngine;

public class AnchorItemThrowable : BaseItemThrowableActivable
{
    protected override void ActivateItem()
    {
        itemActivated = true;

        Debug.Log("Anchor Activated");
    }
}
