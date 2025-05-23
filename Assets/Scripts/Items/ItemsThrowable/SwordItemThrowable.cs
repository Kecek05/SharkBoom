

using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    //DEBUG

    private void OnCollisionEnter2D(Collision2D collision)
    {
        spinObjectComponent.DisableComponent();
    }
}
