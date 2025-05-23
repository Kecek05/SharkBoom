using System.Collections;
using UnityEngine;

public class LifetimeTriggerItemComponent : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Lifetime in seconds
    [SerializeField] private BaseItemThrowable itemThrowable;

    public void StartLifetime()
    {
        StartCoroutine(DelayDestroy());
    }

    private IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(lifetime);

        itemThrowable.DestroyItem();
    }
}
