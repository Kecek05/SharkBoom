using Unity.Netcode;
using UnityEngine;

public class StuckTransformComponent : NetworkBehaviour
{
    [SerializeField] private FollowTransformComponent followTransformComponent;
    private bool stucked = false; //stuck only once

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;

        if (collision.collider.gameObject.TryGetComponent(out IDamageable damageable))
        {
            Stuck(collision.gameObject.transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            Stuck(collision.gameObject.transform);
        }
    }

    private void Stuck(Transform stuckTo)
    {
        if(!stucked)
        {
            stucked = true;
            followTransformComponent.SetTarget(stuckTo);
            followTransformComponent.EnableComponent();
            Debug.Log($"Stuck to {stuckTo.name}");
        }
    }

}
