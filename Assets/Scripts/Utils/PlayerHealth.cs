using QFSW.QC;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private float headMultiplier = 2;
    [SerializeField] private float bodyMultiplier = 1.2f;
    [SerializeField] private float footMultiplier = 0.8f;

    public enum BodyPartEnum
    {
        Head,
        Body,
        Foot
    }

    [Command("health-takeDamage")]
    public void PlayerTakeDamage(float damage, BodyPartEnum bodyPart)
    {
        if(!IsServer) return;

        if (isDead) return;

        switch (bodyPart)
        {
            case BodyPartEnum.Head:
                ModifyHealth(-(damage * headMultiplier));
                Debug.Log(this.gameObject.name + " tomou dano na " + bodyPart + " de " + damage * headMultiplier);
                break;
            case BodyPartEnum.Body:
                ModifyHealth(-(damage * bodyMultiplier));
                Debug.Log(this.gameObject.name + " tomou dano na " + bodyPart + " de " + damage * headMultiplier);
                break;
            case BodyPartEnum.Foot:
                ModifyHealth(-(damage * footMultiplier));
                Debug.Log(this.gameObject.name + " tomou dano na " + bodyPart + " de " + damage * headMultiplier);
                break;
        }
    }

    protected override void Die()
    {
       base.Die(); // make the function on base class "Health"
    }
}
