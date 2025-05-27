using Sortify;
using System.Collections;
using UnityEngine;

public class SpiningObjectComponent : BaseItemComponent
{
    [BetterHeader("References")]
    [SerializeField] private Rigidbody rb;
    [Space(5)]

    [BetterHeader("Settings")]
    [SerializeField] private float spiningSpeed = 300f;

    [Tooltip("Used to invert the direction of rotation")]
    [SerializeField] private bool isInverted = false;
    private Coroutine spinCoroutine;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private float spinDirection = 0;

    protected override void OnEnableComponent()
    {
        StopSpinCoroutine();
    }

    protected override void DoComponentLogic()
    {
        spinCoroutine ??= StartCoroutine(SpinObjectCoroutine()); //if not null start the coroutine and assign it to spinCoroutine
    }

    private IEnumerator SpinObjectCoroutine()
    {
        while (true)
        {
            // Rotate the object around its Z-axis
            if (rb)
            {
                spinDirection = rb.linearVelocity.normalized.x;

                if (spinDirection > 0)
                {
                    transform.Rotate(0, 0, isInverted ? -spiningSpeed : spiningSpeed * Time.deltaTime);
                } else if (spinDirection < 0)
                {
                    transform.Rotate(0, 0, isInverted ? spiningSpeed : -spiningSpeed * Time.deltaTime);
                }
            } else
            {
                // If no Rigidbody, just spin normally
                transform.Rotate(0, 0, isInverted ? -spiningSpeed : spiningSpeed * Time.deltaTime);
            }
            yield return waitForEndOfFrame;
        }
        spinCoroutine = null;
    }

    private void StopSpinCoroutine()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }
    }

    protected override void OnDisableComponent()
    {
        StopSpinCoroutine();
    }
}
