using UnityEngine;

public class PlayerBodyPart : MonoBehaviour, IDamageable
{
    
    public PlayerHealth.BodyPartEnum bodyPart;
    [SerializeField] private Player player;

    public void TakeDamage(float damage)
    {
        player.PlayerHealth.PlayerTakeDamage(damage, bodyPart);
    }
}
