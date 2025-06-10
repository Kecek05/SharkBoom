using System;
using UnityEngine;

public abstract class BaseItemThrowableActivable : BaseItemThrowable
{
    public event Action OnItemActivated;
    protected bool itemActivated = false;
    protected bool itemCanBeActivated = true;
    [SerializeField] protected KnockbackSO knockbackSOActivated;
    [SerializeField] protected HitTriggerComponent hitTriggerComponent;
    protected void OnEnable()
    {
        itemActivated = false;
        itemCanBeActivated = true;
    }
    
    protected override void CollisionController_OnCollidedWithPlayer(PlayerThrower playerObject)
    {
        //Dont allow to activate the item if collided with player
        itemCanBeActivated = false;
    }

    public void TryActivate()
    {
        if(gameObject.activeInHierarchy == false) return; //If the item is not active, don't activate it
        
        if (itemActivated) return;
        
        if(!itemCanBeActivated) return;
        
        itemActivated = true;
        if(knockbackSOActivated)
            if(hitTriggerComponent)
                hitTriggerComponent.SetKnockbackSO(knockbackSOActivated);
        ActivateItem();
        OnItemActivated?.Invoke();
    }


    protected abstract void ActivateItem();

}
