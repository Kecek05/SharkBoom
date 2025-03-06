using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(DamageableSO damageableSO);
}

public interface IDraggable
{
    public void Release(float force, Vector3 direction, Transform shooterTransform);
}


