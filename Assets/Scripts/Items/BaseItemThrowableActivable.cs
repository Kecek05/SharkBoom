using System;
using Sortify;
using UnityEngine;

public abstract class BaseItemThrowableActivable : BaseItemThrowable
{
    public event Action OnItemActivated;
    protected bool itemActivated = false;
    protected bool itemCanBeActivated = true;
    [SerializeField] protected KnockbackSO knockbackSOActivated;
    [SerializeField] protected DamageableSO damageableSOActivated;
    [SerializeField] protected HitTriggerComponent hitTriggerComponent;
    [SerializeField] protected CanDoDamageComponent canDoDamageComponent;
    
    [BetterHeader("Reconcile References", FontSize = 12)]
    [SerializeField] protected DisableCollisionOnCantactComponent disableCollisionOnCantactComponent;
    [SerializeField] protected HideMeshOnCollisionComponent hideMeshOnCollisionComponent;
    [SerializeField] protected SpiningObjectComponent spiningObjectComponent;
    
    protected void OnEnable()
    {
        base.OnEnable();
        itemActivated = false;
        itemCanBeActivated = true;
    }

    public ItemReconcileData GetReconcileData()
    {
        ItemReconcileData reconcileData = new ItemReconcileData
        {
            itemID = itemSO.itemID,
            linearVelocity = rb.linearVelocity,
            angularVelocity = rb.angularVelocity,
            position = transform.position,
            rotation = transform.rotation,
            haveDisableCollisionComponent = disableCollisionOnCantactComponent,
            isCollidersEnabled = disableCollisionOnCantactComponent != null && disableCollisionOnCantactComponent.IsCollidersEnabled,
            haveHideMeshComponent = hideMeshOnCollisionComponent,
            isMeshVisible = hideMeshOnCollisionComponent != null && hideMeshOnCollisionComponent.IsMeshVisible,
            isKinematic = rb.isKinematic,
            haveSpinObjectComponent = spiningObjectComponent,
            isSpinning = spiningObjectComponent != null && spiningObjectComponent.IsSpinning
        };
        // Debug.Log(
        //     $"RECONCILE GET DATA-\n" +
        //     $"Item ID: {reconcileData.itemID}\n" +
        //     $"Linear Velocity: {reconcileData.linearVelocity}\n" +
        //     $"Angular Velocity: {reconcileData.angularVelocity}\n" +
        //     $"Position: {reconcileData.position}\n" +
        //     $"Rotation: {reconcileData.rotation}\n" +
        //     $"Is Kinematic: {reconcileData.isKinematic}\n" +
        //     $"Colliders Enabled: {reconcileData.isCollidersEnabled}\n" +
        //     $"Mesh Visible: {reconcileData.isMeshVisible}\n" +
        //     $"Is Spinning: {reconcileData.isSpinning}"
        // );
        return reconcileData;
    }

    public void Reconcile(ItemReconcileData reconcileData)
    {
        // Debug.Log(
        //     $"RECONCILE RECIVED DATA-\n" +
        //     $"Item ID: {reconcileData.itemID}\n" +
        //     $"Linear Velocity: {reconcileData.linearVelocity}\n" +
        //     $"Angular Velocity: {reconcileData.angularVelocity}\n" +
        //     $"Position: {reconcileData.position}\n" +
        //     $"Rotation: {reconcileData.rotation}\n" +
        //     $"Is Kinematic: {reconcileData.isKinematic}\n" +
        //     $"Colliders Enabled: {reconcileData.isCollidersEnabled}\n" +
        //     $"Mesh Visible: {reconcileData.isMeshVisible}\n" +
        //     $"Is Spinning: {reconcileData.isSpinning}"
        // );
        
        rb.linearVelocity = reconcileData.linearVelocity;
        rb.angularVelocity = reconcileData.angularVelocity;
        transform.position = reconcileData.position;
        transform.rotation = reconcileData.rotation;
        rb.isKinematic = reconcileData.isKinematic;
        
        
        if (disableCollisionOnCantactComponent)
        {
            if (reconcileData.haveDisableCollisionComponent)
            {
                if (reconcileData.isCollidersEnabled)
                {
                    disableCollisionOnCantactComponent.EnableCollisions();
                }
                else
                {
                    disableCollisionOnCantactComponent.DisableCollisions();
                }
            }
        }
        
        if (hideMeshOnCollisionComponent)
        {
            if (reconcileData.haveHideMeshComponent)
            {
                if (reconcileData.isMeshVisible)
                {
                    hideMeshOnCollisionComponent.ShowMesh();
                }
                else
                {
                    hideMeshOnCollisionComponent.HideMesh();
                }
            }
        }
        
        if (spiningObjectComponent)
        {
            if (reconcileData.haveSpinObjectComponent)
            {
                if (reconcileData.isSpinning)
                {
                    spiningObjectComponent.StartComponentLogic();
                }
                else
                {
                    spiningObjectComponent.StopSpinCoroutine();
                }
            }
        }
        
        TriggerActivation();
    }
    
    protected override void CollisionController_OnCollidedWithPlayer(PlayerThrower playerObject)
    {
        //Dont allow to activate the item if collided with player
        itemCanBeActivated = false;
    }

    /// <summary>
    /// Try To Activate the item. If cant, will do nothing.
    /// </summary>
    public void TryActivate(Action<bool> sucessCallback = null)
    {
        if (gameObject.activeInHierarchy == false)
        {
            sucessCallback?.Invoke(false);
            return; //If the item is not active, don't activate it
        }

        if (ownerPlayableState != turnManager.LocalPlayableState)
        {
            sucessCallback?.Invoke(false);
            return; // Trying to activate an item that is not owned by the local player
        }

        if (itemActivated)
        {
            sucessCallback?.Invoke(false);
            return;
        }

        if (!itemCanBeActivated)
        {
            sucessCallback?.Invoke(false);
            return;
        }
        
        sucessCallback?.Invoke(true);
        TriggerActivation();
    }

    /// <summary>
    /// Trigger the activation of the item, calling it WILL activate the item, so be careful.
    /// </summary>
    private void TriggerActivation()
    {
        itemActivated = true;
        if(knockbackSOActivated)
            if(hitTriggerComponent)
                hitTriggerComponent.SetKnockbackSO(knockbackSOActivated);
        
        ActivateItem();
        OnItemActivated?.Invoke();
        
        // Debug.Log("RECONCILE - Trigger Activation called for item: " + itemSO.itemID);
    }

    protected abstract void ActivateItem();

}
