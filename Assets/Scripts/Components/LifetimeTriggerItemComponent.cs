using System.Collections;
using UnityEngine;

public class LifetimeTriggerItemComponent : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Lifetime in seconds
    [SerializeField] private BaseItemThrowable itemThrowable;

    //DEBUG
    public bool isActive = false;

    public void StartLifetime()
    {
        StartCoroutine(DelayDestroy());
    }

    private IEnumerator DelayDestroy()
    {
        isActive = true;
        yield return new WaitForSeconds(lifetime);

        itemThrowable.DestroyItem();
    }
}
