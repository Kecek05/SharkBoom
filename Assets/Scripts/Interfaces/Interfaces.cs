using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage, float damageMultiplier = 1f);

    public void Die();
}

public interface IDraggable
{
    public void Release(float force, Vector3 direction);
}

public interface IUseable
{
    public void Use();
}
