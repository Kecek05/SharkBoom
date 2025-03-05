using UnityEngine;

public class PlayerDamageControl : MonoBehaviour
{
    public enum BodyPartEnum
    {
        Head,
        Body,
        Foot
    }
    public BodyPartEnum BodyPart;

    [SerializeField] private Collider bodyPartCollision;
    [SerializeField] private Health health;

    public void CalculateDamage(float damage)
    {
        if (BodyPart == BodyPartEnum.Head)
        {
            health.TakeDamage(damage, 2f);
        }
        else if (BodyPart == BodyPartEnum.Body)
        {
            health.TakeDamage(damage, 1.3f);
        }
        else if (BodyPart == BodyPartEnum.Foot)
        {
            health.TakeDamage(damage, 0.8f);
        }
    }
    
}
