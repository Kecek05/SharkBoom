using QFSW.QC;
using UnityEngine;

public class PlayerHealth : Health
{

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

        selectedMultiplier = bodyPart == BodyPartEnum.Head ? damageableSO.headMultiplier : bodyPart == BodyPartEnum.Body ? damageableSO.bodyMultiplier : bodyPart == BodyPartEnum.Foot ? damageableSO.footMultiplier : 0f; //0f error

        if(selectedMultiplier == 0f)
        {
            Debug.LogWarning("Bodypart not found");
            return;
        }

        ModifyHealth(-(damageableSO.damage * selectedMultiplier));

        Debug.Log($"Damage: {damageableSO.damage} in: {bodyPart} with multiplier: {selectedMultiplier} total: {damageableSO.damage * selectedMultiplier}");
    }

    protected override void Die()
    {
       base.Die(); // make the function on base class "Health"
    }
}
