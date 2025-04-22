using UnityEngine;

public class LifetimeTriggerComponent : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Lifetime in seconds

    public void StartLifetime()
    {
        Destroy(gameObject, lifetime);
    }
}
