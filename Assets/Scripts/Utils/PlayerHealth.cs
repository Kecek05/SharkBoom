using QFSW.QC;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private float headMultiplier = 2;
    [SerializeField] private float bodyMultiplier = 1f;
    [SerializeField] private float footMultiplier = 0.8f;
    private float selectedMultiplier; //cache

    public enum BodyPartEnum
    {
        Head,
        Body,
        Foot
    }

    [Command("health-takeDamage")]
    public void PlayerTakeDamage(DamageableSO damageableSO, BodyPartEnum bodyPart)
    {
        if(!IsServer) return;

        if (isDead) return;

        selectedMultiplier = bodyPart == BodyPartEnum.Head ? headMultiplier : bodyPart == BodyPartEnum.Body ? bodyMultiplier : bodyPart == BodyPartEnum.Foot ? footMultiplier : 0f; //0f error

        if(selectedMultiplier == 0f)
        {
            Debug.LogWarning("Bodypart not found");
            return;
        }

        ModifyHealth(-(damageableSO.damage * selectedMultiplier));

        Debug.Log($"Damage: {damageableSO.damage} in: {bodyPart} with multiplier: {selectedMultiplier} total: {damage * selectedMultiplier}");
    }

    protected override void Die()
    {
       base.Die(); // make the function on base class "Health"
    }
}
