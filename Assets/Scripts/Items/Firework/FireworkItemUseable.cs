using Sortify;
using UnityEngine;

public class FireworkItemUseable : MonoBehaviour, IUseable
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float force = 10f;

    public void Use()
    {
        UseFirework();
    }

    protected virtual void UseFirework()
    {
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        Debug.Log("Firework used!");
    }
}
