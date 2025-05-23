using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseItemThrowable : NetworkBehaviour
{
    public static event Action OnItemFinishedAction;
    public static event Action<Transform> OnItemReleasedAction;
    public static event Action OnItemCallbackAction;

    [BetterHeader("Base Item References")]
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected GameObject[] objectsToChangeLayer;
    [SerializeField] protected DissolveShaderComponent dissolveShaderComponent;
    [SerializeField] protected LifetimeTriggerItemComponent lifetimeTriggerItemComponent;
    [SerializeField] protected FollowTransformComponent followTransformComponent; //Used to follow the hand when the item is in hand
    [SerializeField] protected NetworkObject myNetworkObject;
    protected ItemLauncherData thisItemLaucherData;

    protected BaseTurnManager turnManager;

    protected bool itemReleased = false;

    //DEBUG
    public bool IsItemReleased => itemReleased;

    /// <summary>
    /// Called when the item spawns in hand
    /// </summary>
    /// <param name="itemLauncherData"></param>
    public virtual void Initialize(Transform parent)
    {
        if(!IsOwner) return; //Only the owner can initialize the item

        rb.isKinematic = true; //Set the item to kinematic until the item is released

        if(parent != null)
        {
            followTransformComponent.SetTarget(parent);
            followTransformComponent.EnableComponent();
        }

        if (dissolveShaderComponent != null)
            dissolveShaderComponent.DissolveFadeIn();

        InitializeUpdateRbTypeServerRpc(true);

    }

    [Rpc(SendTo.Server)]
    private void InitializeUpdateRbTypeServerRpc(bool isKinematic)
    {
        InitializeUpdateRbTypeClientRpc(isKinematic);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void InitializeUpdateRbTypeClientRpc(bool isKinematic)
    {
        if(IsOwner) return; //Ownler already changed

        rb.isKinematic = isKinematic;
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
        if(!IsOwner) return; //Only the owner can release the item

        UpdateOnRelease(itemLauncherData);

        followTransformComponent.DisableComponent();
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        rb.AddForce(itemLauncherData.dragDirection * itemLauncherData.dragForce, ForceMode.Impulse);

        if(lifetimeTriggerItemComponent)
            lifetimeTriggerItemComponent.StartLifetime();
        
        ItemReleasedServerRpc(itemLauncherData);
    }

    [Rpc(SendTo.Server)]
    private void ItemReleasedServerRpc(ItemLauncherData itemLauncherData)
    {
        Debug.Log("ItemReleasedServerRpc");
        
        UpdateOnRelease(itemLauncherData);

        ItemReleasedClientRpc(itemLauncherData);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ItemReleasedClientRpc(ItemLauncherData itemLauncherData)
    {
        if(IsOwner) return; //Owner already released

        UpdateOnRelease(itemLauncherData);
    }

    private void UpdateOnRelease(ItemLauncherData itemLauncherData)
    {
        itemReleased = true;

        SetCollision(itemLauncherData.ownerPlayableState);
        thisItemLaucherData = itemLauncherData;

        OnItemReleasedAction?.Invoke(this.transform);
        rb.isKinematic = false;
    }

    private void SetCollision(PlayableState playableState)
    {
        switch (playableState)
        {
            case PlayableState.Player1Playing:
                foreach (GameObject gameObject in objectsToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_1_LAYER;
                }
                break;
            case PlayableState.Player2Playing:
                foreach (GameObject gameObject in objectsToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.PLAYER_2_LAYER;
                }
                break;
            case PlayableState.None:
                foreach (GameObject gameObject in objectsToChangeLayer)
                {
                    gameObject.layer = PlayersPublicInfoManager.NO_DEFINED_PLAYER_LAYER;
                }
                break;
        }
        Debug.Log($"SetCollision: {playableState} | Layer: {gameObject.layer}");
    }

    protected virtual void ItemCallbackAction()
    {
        if(!IsOwner) return; // Only the server should call the callback action

        turnManager.PlayerPlayed(thisItemLaucherData.ownerPlayableState);
        ItemCallbackServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void ItemCallbackServerRpc()
    {
        ItemCallbackClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ItemCallbackClientRpc()
    {
        OnItemCallbackAction?.Invoke();
    }

    private void ResetItemThrowableState()
    {
        SetCollision(PlayableState.None);
        itemReleased = false;
        followTransformComponent.DisableComponent();
        rb.isKinematic = false;
    }


    public virtual void DestroyItem(Action destroyedCallback = null)
    {
        if (!IsOwner) return; //Only the owner can destroy the item

        OnItemFinishedAction?.Invoke();

        if (itemReleased)
            ItemCallbackAction();

        if (dissolveShaderComponent != null)
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
        SetCollision(PlayableState.None); //Set the collision to none
        NetworkObjectPool.Instance.ReturnNetworkObject(myNetworkObject, itemSO.itemIndex); // Return the object to the pool
        myNetworkObject.Despawn(true); // Pass 'true' to also destroy the GameObject

        DestroyOnClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyOnClientRpc()
    {
        ResetItemThrowableState();
    }
}
