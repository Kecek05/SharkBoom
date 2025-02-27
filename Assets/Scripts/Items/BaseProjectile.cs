using UnityEngine;

public class BaseProjectile : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;

    void Start()
    {
        
    }

   
    void Update()
    {
        rb.linearVelocity = Vector3.right * 2f;
    }
}
