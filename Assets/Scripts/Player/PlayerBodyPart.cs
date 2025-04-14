using UnityEngine;

public class PlayerBodyPart : MonoBehaviour, IDamageable
{
    
    [SerializeField] private BodyPartEnum bodyPart;
    [SerializeField] private PlayerHealth playerHealth;
    public void TakeDamage(DamageableSO damageableSO)
    {
        playerHealth.PlayerTakeDamage(damageableSO, bodyPart);
    }
}
