using System;
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
    [SerializeField] private PlayerThrower player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsClient)
        {
            currentHealth.OnValueChanged += CurrentHealth_OnValueChanged;
        }
    }

    private void CurrentHealth_OnValueChanged(float previousValue, float newValue)
    {
        OnPlayerTakeDamage?.Invoke(this, new OnPlayerTakeDamageArgs { playableState = player.ThisPlayableState.Value, playerCurrentHealth = currentHealth.Value, playerMaxHealth = maxHealth });
    }



    public void PlayerTakeDamage(DamageableSO damageableSO, BodyPartEnum bodyPart)
    {
        if (IsServer && !isDead)
        {
            selectedMultiplier = bodyPart == BodyPartEnum.Head ? damageableSO.headMultiplier : bodyPart == BodyPartEnum.Body ? damageableSO.bodyMultiplier : bodyPart == BodyPartEnum.Foot ? damageableSO.footMultiplier : 0f; //0f error

            if(selectedMultiplier == 0f)
            {
                Debug.LogWarning("Bodypart not found");
                return;
            }

            Debug.Log($"Damage: {damageableSO.damage} in: {bodyPart} with multiplier: {selectedMultiplier} total: {damageableSO.damage * selectedMultiplier} damageableSO: {damageableSO}");

            ModifyHealth(-(damageableSO.damage * selectedMultiplier));

        }

    }

    protected override void Die()
    {
        base.Die(); // call the function on base class "Health"

        if (!IsServer) return;

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
