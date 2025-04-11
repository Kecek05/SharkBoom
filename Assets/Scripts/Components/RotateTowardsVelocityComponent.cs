using UnityEngine;

public class RotateTowardsVelocityComponent : MonoBehaviour
{
    [Tooltip("How fast the spear rotates to align with its velocity.")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody2D rb;

    private void FixedUpdate()
    {
        Vector2 vel2D = rb.linearVelocity;
        if (vel2D.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(vel2D.y, vel2D.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
