using UnityEngine;

public class C4Item : BaseItemThrowableActiveable
{
    [SerializeField] private GameObject explosionObj;

    protected override void ActivateItem()
    {
        itemActivated = true;

        explosionObj.SetActive(true);
        Debug.Log("C4Item Activated");
    }
}
