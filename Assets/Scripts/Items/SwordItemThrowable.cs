

using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void Initialize(ItemLauncherData itemLauncherData)
    {
        base.Initialize(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }


    //DEBUG

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    spinObjectComponent.DisableComponent();

    //    rb.freezeRotation = true; //freeze rotation to avoid the spear to rotate when it hits something DEBUG
    //}
}
