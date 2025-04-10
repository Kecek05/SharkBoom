using UnityEngine;

public class C4Item : BaseItemThrowableActivable
{
    [SerializeField] private GameObject explosionObj;

    protected override void ActivateItem()
    {
        itemActivated = true;

        explosionObj.SetActive(true);
    }

    private void OnDestroy()
    {
        Debug.Log("C4 destroyed");
    }
}
