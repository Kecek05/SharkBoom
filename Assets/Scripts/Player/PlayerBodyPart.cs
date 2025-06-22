using UnityEngine;

public class PlayerBodyPart : MonoBehaviour, IDamageable, ILocalDamageable
{
    
    [SerializeField] private BodyPartEnum bodyPart;
    [SerializeField] private PlayerHealth playerHealth;
    public void TakeDamage(DamageableSO damageableSO)
    {
        // Debug.Log($"HEALTH - Take Damage: {damageableSO} - {gameObject.transform.parent.name}");
        playerHealth.PlayerTakeDamage(damageableSO, bodyPart);
    }

    public void TakeLocalDamage(DamageableSO damageableSO)
    {
        // Debug.Log($"HEALTH - Take Local Damage: {damageableSO} - {gameObject.transform.parent.name}");
        playerHealth.PlayerTakeLocalDamage(damageableSO, bodyPart);
    }
}
