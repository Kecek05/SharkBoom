using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : HealthComponent
{
    
    public static event EventHandler<OnPlayerTakeDamageArgs> OnPlayerTakeDamage;
    
    public class OnPlayerTakeDamageArgs : EventArgs
    {
        public PlayableState playableState;
        public float playerCurrentHealth;
        public float playerMaxHealth;
    }

    /// <summary>
    /// Server is who calls this event.
    /// </summary>
    public static event Action OnPlayerDie;

    private float selectedMultiplier; //cache
    private float localSelectedMultiplier;
    [SerializeField] private PlayerThrower player;
    private float delayLocalPlayerDie = 2f; //seconds
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsClient)
        {
            currentHealth.OnValueChanged += CurrentHealth_OnValueChanged;
            CurrentHealth_OnValueChanged(0f, currentHealth.Value);
        }
    }

    public void InitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction += BaseItemThrowableOnOnItemCallbackAction;
    }

    public void UnInitializeOwner()
    {
        BaseItemThrowable.OnItemCallbackAction -= BaseItemThrowableOnOnItemCallbackAction;
    }

    private void BaseItemThrowableOnOnItemCallbackAction(bool isOwnerOfItem)
    {
        // if(IsHost) return; //Host is always synced
        // Debug.Log($"HEALTH 1 - Item Callback - Syncronized This turn is false - {gameObject.name}");

        //Sync local health with localTargetHealth;
        SetLocalHealth(localTargetHealth);
    }

    //Its possible to recieve the callback of an item before the value change of the Current health
    
    private void CurrentHealth_OnValueChanged(float previousValue, float newValue)
    {
        //Syncronize localhealth with currentHealth
        // Debug.Log($"HEALTH 2 - CurrentHealth_OnValueChanged - Syncronized This turn is true - localTargetHealth is: {newValue} - {gameObject.name}");
        localTargetHealth = newValue;
    }

    public void PlayerTakeLocalDamage(DamageableSO damageableSO, BodyPartEnum bodyPart)
    {
        localSelectedMultiplier = bodyPart == BodyPartEnum.Head ? damageableSO.headMultiplier : bodyPart == BodyPartEnum.Body ? damageableSO.bodyMultiplier : bodyPart == BodyPartEnum.Foot ? damageableSO.footMultiplier : 0f; //0f error

        if(localSelectedMultiplier == 0f)
        {
            Debug.LogWarning("Bodypart not found");
            return;
        }

        // Debug.Log($"HEALTH - Damage LOCAL: {damageableSO.damage} in: {bodyPart} with multiplier: {localSelectedMultiplier} total: {damageableSO.damage * localSelectedMultiplier} damageableSO: {damageableSO} - {gameObject.name}");

        ModifyLocalHealth(-(damageableSO.damage * localSelectedMultiplier));
    }

    public void PlayerTakeDamage(DamageableSO damageableSO, BodyPartEnum bodyPart)
    {
        if (!isDead.Value)
        {
            selectedMultiplier = bodyPart == BodyPartEnum.Head ? damageableSO.headMultiplier : bodyPart == BodyPartEnum.Body ? damageableSO.bodyMultiplier : bodyPart == BodyPartEnum.Foot ? damageableSO.footMultiplier : 0f; //0f error

            if(selectedMultiplier == 0f)
            {
                Debug.LogWarning("Bodypart not found");
                return;
            }

            // Debug.Log($"HEALTH - Damage: {damageableSO.damage} in: {bodyPart} with multiplier: {selectedMultiplier} total: {damageableSO.damage * selectedMultiplier} damageableSO: {damageableSO} - {gameObject.name}");

            ModifyHealthServerRpc(-(damageableSO.damage * selectedMultiplier));

        }

    }
    
    protected override void InvokeOnLocalHealthChanged()
    {
        OnPlayerTakeDamage?.Invoke(this, new OnPlayerTakeDamageArgs { playableState = player.ThisPlayableState.Value, playerCurrentHealth = localCurrentHealth, playerMaxHealth = maxHealth });

        if (localCurrentHealth <= 0)
        {
            //Died - Show Game UI
            StartCoroutine(DelayToInvokeLocalDie());
        }
    }
    
    private IEnumerator DelayToInvokeLocalDie()
    {
        yield return new WaitForSecondsRealtime(delayLocalPlayerDie);
        InvokeOnLocalDie();
    }
    
    protected override void Die()
    {
        if (!IsServer) return;
        // Debug.Log($"DIE - PLAYER DIE");
        OnPlayerDie?.Invoke();

    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentHealth.OnValueChanged -= CurrentHealth_OnValueChanged;
        }
    }
}

public enum BodyPartEnum
{
    Head,
    Body,
    Foot
}
