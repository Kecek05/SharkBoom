using UnityEngine;

public class CoconutItemThrowable : BaseItemThrowable
{
    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void ItemReleased(ItemLauncherData itemLauncherData)
    {
        base.ItemReleased(itemLauncherData);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }

    private void OnCollisionEnter(Collision collision)
    {
        spinObjectComponent.DisableComponent();
    }
}
