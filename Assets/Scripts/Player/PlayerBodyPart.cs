using UnityEngine;

public class PlayerBodyPart : MonoBehaviour, IDamageable, ILocalDamageable
{
    
    [SerializeField] private BodyPartEnum bodyPart;
    [SerializeField] private PlayerHealth playerHealth;
    public void TakeDamage(DamageableSO damageableSO)
    {
        playerHealth.PlayerTakeDamage(damageableSO, bodyPart);
    }

    public void TakeLocalDamage(DamageableSO damageableSO)
    {
        playerHealth.PlayerTakeLocalDamage(damageableSO, bodyPart);
    }
}
