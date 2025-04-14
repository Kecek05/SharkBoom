using UnityEngine;

public class LifetimeComponent : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    // Debug.Log
}
