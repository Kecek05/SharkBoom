using UnityEngine;

public class SpiningObjectComponent : MonoBehaviour
{
    [SerializeField] private float spiningSpeed = 300f;

    private void FixedUpdate()
    {
        // Rotate the object around its Z-axis
        transform.Rotate(0, 0, -spiningSpeed * Time.deltaTime);
    }
}
