using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);

    public void Die();
}

public interface IDraggable
{
    public void Release(float force, Vector3 direction);
}
