using Sortify;
using Unity.Netcode;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour, IDraggable
{
    [BetterHeader("References")]
    [SerializeField] protected bool isServerObject;
    [SerializeField] protected bool canDoDamage = true;
    [SerializeField] protected ItemSO itemSO;
    [SerializeField] protected Rigidbody rb;
    protected Transform shooterTransform;


    public void Release(float force, Vector3 direction, Transform _shooterTransform)
    {
        shooterTransform = _shooterTransform;
        ItemReleased(force, direction);
    }

    protected virtual void ItemReleased(float force, Vector3 direction)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isServerObject || !canDoDamage)
        {
            //Change latter
            Destroy(gameObject);
            return;
        }

        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                damageable.TakeDamage(itemSO.damageableSO);
                Debug.Log("Dealt " + itemSO.damageableSO.damage + " damage to " + collision.gameObject.name);
            }
        }

    }
}
