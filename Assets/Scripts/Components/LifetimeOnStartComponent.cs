using UnityEngine;

public class LifetimeOnStartComponent : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    // Debug.Log
}
