using Sortify;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseItemThrowable : MonoBehaviour
{
    private const float ITEM_Z_POSITION = -14.5f;
    
    /// <summary>
    /// Called when the item is destroyed.
    /// </summary>
    public static event Action OnItemFinishedAction;
    /// <summary>
    /// Called when the item is released from the hand.
    /// </summary>
    public static event Action<Transform> OnItemReleasedAction;
    /// <summary>
    /// Called when the item is destroyed and was released.
    /// </summary>
    public static event Action OnItemCallbackAction;

    [BetterHeader("Base Item References")]
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected GameObject[] objectsToChangeLayer;
    [SerializeField] protected DissolveShaderComponent dissolveShaderComponent;
    [SerializeField] protected LifetimeTriggerItemComponent lifetimeTriggerItemComponent;
    [SerializeField] protected FollowTransformComponent followTransformComponent; //Used to follow the hand when the item is in hand
    [Tooltip("Can Be Null | Used to Listen to Collision Events")] 
    [SerializeField] protected BaseCollisionController collisionController;
    [Space(4)]
    [BetterHeader("Item Constraints Settings", 12)]
    [SerializeField] protected bool freezePositionX = false;
    [SerializeField] protected bool freezePositionY = false;
    [SerializeField] protected bool freezePositionZ = false;
    [SerializeField] protected bool freezeRotationX = true;
    [SerializeField] protected bool freezeRotationY = true;
    [SerializeField] protected bool freezeRotationZ = false;
    [Space(6)]

    protected ItemLauncherData thisItemLaucherData;

    protected BaseTurnManager turnManager;

    protected bool itemReleased = false;
    
    protected PlayableState ownerPlayableState;
    
    protected bool isOwner = false;
    protected bool initialized = false;
    
    public bool IsOwner => isOwner;
    public bool Initialized => initialized;

    //DEBUG
    public bool IsItemReleased => itemReleased;

    protected void OnEnable()
    {
        ownerPlayableState = PlayableState.None;
    }

    /// <summary>
    /// Called when the item spawns in hand
    /// </summary>
    /// <param name="itemLauncherData"></param>
    public virtual void Initialize(Transform parent)
    {
        rb.isKinematic = true; //Set the item to kinematic until the item is released

        if(parent)
        {
            followTransformComponent.SetTarget(parent);
            followTransformComponent.EnableComponent();
        }

        if (dissolveShaderComponent)
            dissolveShaderComponent.DissolveFadeIn();

        if(collisionController)
        {
            collisionController.OnCollided += CollisionController_OnCollided;
            collisionController.OnCollidedWithPlayer += CollisionController_OnCollidedWithPlayer;
            collisionController.OnCollidedWithoutPlayer += CollisionController_OnCollidedWithoutPlayer;
        }

        ResetConstraints();
        initialized = true;
    }

    protected virtual void CollisionController_OnCollidedWithoutPlayer(GameObject collidedObject)
    {
        
    }

    protected virtual void CollisionController_OnCollidedWithPlayer(PlayerThrower playerObject)
    {

    }

    protected virtual void CollisionController_OnCollided(GameObject collidedObject)
    {
        
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
    public virtual void ItemReleased(ItemLauncherData itemLauncherData, bool isOwner)
    {
        Debug.Log($"STEPS - ITEM RELEASED - IS OWNER: {isOwner} - Owner State: {itemLauncherData.ownerPlayableState}");
        this.isOwner = isOwner;
        ownerPlayableState = itemLauncherData.ownerPlayableState;
        turnManager = ServiceLocator.Get<BaseTurnManager>();
        UpdateOnRelease(itemLauncherData);

        followTransformComponent.DisableComponent();
        transform.position = new Vector3(transform.position.x, transform.position.y, ITEM_Z_POSITION);
        rb.interpolation = RigidbodyInterpolation.Interpolate; //force it
        rb.AddForce(itemLauncherData.dragDirection * itemLauncherData.dragForce, ForceMode.Impulse);

        if(lifetimeTriggerItemComponent)
            lifetimeTriggerItemComponent.StartLifetime();
        
    }

    private void UpdateOnRelease(ItemLauncherData itemLauncherData)
    {
        itemReleased = true;

        SetCollision(itemLauncherData.ownerPlayableState);
        thisItemLaucherData = itemLauncherData;

        OnItemReleasedAction?.Invoke(this.transform);
        rb.isKinematic = false;
        transform.localEulerAngles = new Vector3(0f, 0f, 0f); // reset rotation
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
    }

    protected virtual void ItemCallbackAction()
    {
        //if(!IsOwner) return; // Only the server should call the callback action

        //FireItemCallbackAction();
        
        OnItemCallbackAction?.Invoke();
        
        if(isOwner)
            turnManager.PlayerPlayed(thisItemLaucherData.ownerPlayableState);
    }

    /// <summary>
    /// Called when the object is Destroyed, all clients call this. Use to reset to the initial state of the item to the pool.
    /// </summary>
    protected virtual void ResetItemThrowableState()
    {
        SetCollision(PlayableState.None);
        itemReleased = false;
        followTransformComponent.DisableComponent();
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ResetConstraints();
        initialized = false;
        //Debug.Log("Item Reseted to Pool");
    }

    private void ResetConstraints()
    {
        if(freezePositionX)
            rb.constraints |= RigidbodyConstraints.FreezePositionX; // Adds the FreezePositionX constraint without removing others.
        else
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX; // Removes FreezePositionX while preserving others.

        if (freezePositionY)
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;

        if (freezePositionZ)
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;

        if (freezeRotationX)
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;

        if (freezeRotationY)
            rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;

        if (freezeRotationZ)
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        else
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
    }

    public virtual void DestroyItem(Action destroyedCallback = null)
    {
        OnItemFinishedAction?.Invoke();

        if (itemReleased)
            ItemCallbackAction();
        
        //Debug.Log($"ITEM - Destroying item {itemSO.itemID} of type {itemSO.itemName} - Have dissolve: {dissolveShaderComponent != null}");
        if (dissolveShaderComponent)
        {
            dissolveShaderComponent.DissolveFadeOut(() =>
            {
                destroyedCallback?.Invoke();
                ObjectPool.Instance.ReturnObject(gameObject, itemSO.itemID);
            });
        }
        else
        {
            destroyedCallback?.Invoke();
            ObjectPool.Instance.ReturnObject(gameObject, itemSO.itemID);
        }
    }

    private void OnDisable()
    {
        if (collisionController)
        {
            collisionController.OnCollided -= CollisionController_OnCollided;
            collisionController.OnCollidedWithPlayer -= CollisionController_OnCollidedWithPlayer;
        }
        ResetItemThrowableState();
    }
}
