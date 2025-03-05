using Sortify;
using UnityEngine;

public class BaseItemThrowable : MonoBehaviour, IDraggable
{
    [BetterHeader("References")]
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
}
