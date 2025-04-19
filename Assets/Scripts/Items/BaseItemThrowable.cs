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

    /// <summary>
    /// Called when the item spawns in hand
    /// </summary>
    /// <param name="itemLauncherData"></param>
    public virtual void Initialize(PlayableState ownerPlayableState)
    {
        //thisItemLaucherData = itemLauncherData;
        rb.

        switch(ownerPlayableState)
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

        //turnManager = ServiceLocator.Get<BaseTurnManager>();
        //ItemReleased(thisItemLaucherData.dragForce, thisItemLaucherData.dragDirection);
    }

    /// <summary>
    /// Called when the item should stop following the hand and will be released
    /// </summary>
    /// <param name="force"></param>
    /// <param name="direction"></param>
    protected virtual void ItemReleased(ItemLauncherData itemLauncherData)
    {
        thisItemLaucherData = itemLauncherData;
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        OnItemReleasedAction?.Invoke(this.transform);
        rb.AddForce(itemLauncherData.dragDirection * itemLauncherData.dragForce, ForceMode2D.Impulse);
        
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
