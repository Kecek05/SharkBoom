using Sortify;
using System;
using UnityEngine;

public abstract class BaseItemThrowable : MonoBehaviour
{

    public static event Action OnItemFinishedAction;
    public static event Action<Transform> OnItemReleasedAction;

    [BetterHeader("Base Item References")]
    [SerializeField] protected bool isServerObject;
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected GameObject[] collidersToChangeLayer;
    protected ItemLauncherData thisItemLaucherData;

    protected BaseTurnManager turnManager;

    public virtual void Initialize(ItemLauncherData itemLauncherData)
    {
        thisItemLaucherData = itemLauncherData;

        switch(thisItemLaucherData.ownerPlayableState)
        {
            case PlayableState.Player1Playing:
                foreach(GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_1_LAYER;
                }
                break;
            case PlayableState.Player2Playing:
                foreach (GameObject gameObject in collidersToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_2_LAYER;
                }
                break;
        }

        turnManager = ServiceLocator.Get<BaseTurnManager>();
        ItemReleased(thisItemLaucherData.dragForce, thisItemLaucherData.dragDirection);
    }


    protected virtual void ItemReleased(float force, Vector2 direction)
    {
        OnItemReleasedAction?.Invoke(this.transform);
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        
    }

    protected virtual void ItemCallbackAction()
    {
        if(!isServerObject) return; // Only the server should call the callback action

        turnManager.PlayerPlayed(thisItemLaucherData.ownerPlayableState);
    }

    protected void OnDestroy()
    {
        
        OnItemFinishedAction?.Invoke();
        ItemCallbackAction();
    }
}
