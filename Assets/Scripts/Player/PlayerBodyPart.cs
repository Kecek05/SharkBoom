using UnityEngine;

public class PlayerBodyPart : MonoBehaviour, IDamageable
{
    
    [SerializeField] private BodyPartEnum bodyPart;
    [SerializeField] private Player player;
    public void TakeDamage(DamageableSO damageableSO)
    {
        player.PlayerHealth.PlayerTakeDamage(damageableSO, bodyPart);
    }
}
