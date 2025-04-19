

using UnityEngine;

public class SwordItemThrowable : BaseItemThrowable
{

    [SerializeField] private BaseItemComponent spinObjectComponent;

    public override void Initialize(PlayableState ownerPlayableState)
    {
        base.Initialize(ownerPlayableState);

        spinObjectComponent.EnableComponent();

        spinObjectComponent.StartComponentLogic();
    }


    //DEBUG

    private void OnCollisionEnter2D(Collision2D collision)
    {
        spinObjectComponent.DisableComponent();
    }
}
