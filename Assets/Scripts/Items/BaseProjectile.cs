using Sortify;
using UnityEngine;

public class BaseProjectile : MonoBehaviour, IDraggable
{
    [BetterHeader("References")]
    [SerializeField] private Rigidbody rb;


    public void Release(float force, Vector3 direction)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}
