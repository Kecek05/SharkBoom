using Sortify;
using Unity.Netcode;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour, IDraggable
{
    [BetterHeader("References")]
    [Toggle][SerializeField] protected bool isServerObject;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServerObject)
        {
            //Change latter
            Destroy(gameObject);
            return;
        }

        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (NetworkManager.Singleton.IsServer)
            {
                damageable.TakeDamage(itemSO.damage);
                Debug.Log("Dealt " + itemSO.damage + " damage to " + collision.gameObject.name);
            }
        }

    }
}
