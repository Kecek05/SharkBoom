using UnityEngine;

public class DestroyOnAnyContact : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
