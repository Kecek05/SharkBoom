using UnityEngine;

public class DestroyOnAnyContact : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Destroying object on collision");
        Destroy(gameObject);
    }
}
