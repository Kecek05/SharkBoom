using UnityEngine;

public class DestroyOnAnyContact : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
