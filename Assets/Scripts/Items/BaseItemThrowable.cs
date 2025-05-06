using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseItemThrowable : NetworkBehaviour
{

    public static event Action OnItemFinishedAction;
    public static event Action<Transform> OnItemReleasedAction;

    [BetterHeader("Base Item References")]
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected GameObject[] collidersToChangeLayer;
    [SerializeField] protected DissolveShaderComponent dissolveShaderComponent;
    [SerializeField] protected LifetimeTriggerItemComponent lifetimeTriggerItemComponent;
    [SerializeField] protected FollowTransformComponent followTransformComponent; //Used to follow the hand when the item is in hand
    [SerializeField] protected NetworkObject myNetworkObject;
    protected ItemLauncherData thisItemLaucherData;

    protected BaseTurnManager turnManager;

    protected bool itemReleased = false;

    /// <summary>
    /// Called when the item spawns in hand
    /// </summary>
    /// <param name="itemLauncherData"></param>
    public virtual void Initialize(Transform parent)
    {
        rb.bodyType = RigidbodyType2D.Static; //Statick until the item is released

        followTransformComponent.SetTarget(parent);
        followTransformComponent.EnableComponent();

        if (dissolveShaderComponent != null)
            dissolveShaderComponent.DissolveFadeIn();

    }

    public virtual void ChangeFollowTransform(Transform follow)
    {
        followTransformComponent.SetTarget(follow);
    }

    /// <summary>
    /// Called when the item should stop following the hand and will be released
    /// </summary>
    /// <param name="force"></param>
    /// <param name="direction"></param>
    public virtual void ItemReleased(ItemLauncherData itemLauncherData)
    {
        itemReleased = true;

        SetCollision(itemLauncherData.ownerPlayableState);
        followTransformComponent.DisableComponent();
        thisItemLaucherData = itemLauncherData;
        turnManager = ServiceLocator.Get<BaseTurnManager>();

        OnItemReleasedAction?.Invoke(this.transform);
        rb.bodyType = RigidbodyType2D.Dynamic; //Statick until the item is released
        rb.AddForce(itemLauncherData.dragDirection * itemLauncherData.dragForce, ForceMode2D.Impulse);

        if(lifetimeTriggerItemComponent)
            lifetimeTriggerItemComponent.StartLifetime();
    }

    private void SetCollision(PlayableState playableState)
    {
        switch (playableState)
        {
            case PlayableState.Player1Playing:
                foreach (GameObject gameObject in collidersToChangeLayer)
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
    }

    protected virtual void ItemCallbackAction()
    {
        if(!IsServer) return; // Only the server should call the callback action

        turnManager.PlayerPlayed(thisItemLaucherData.ownerPlayableState);
    }

    public virtual void DestroyItem(Action destroyedCallback = null)
    {
        OnItemFinishedAction?.Invoke();

        if(itemReleased)
            ItemCallbackAction();

        if(dissolveShaderComponent != null)
        {
            dissolveShaderComponent.DissolveFadeOut(() =>
            {
                DestroyOnServerRpc();
                destroyedCallback?.Invoke();
                dissolveShaderComponent = null;
            });
        }
        else
        {
            DestroyOnServerRpc();
            destroyedCallback?.Invoke();
        }
    }

    [Rpc(SendTo.Server)]
    private void DestroyOnServerRpc()
    {
        Debug.Log("DESTROY ITEM");
        myNetworkObject.Despawn(true); // Pass 'true' to also destroy the GameObject
    }

    public override void OnDestroy()
    {
        DestroyItem();
    }
}
