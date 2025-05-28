using Sortify;
using System.Collections;
using UnityEngine;

public class SpiningObjectComponent : BaseItemComponent
{
    [BetterHeader("References")]
    [SerializeField] private Rigidbody rb;
    [Space(5)]

    [BetterHeader("Settings")]
    [SerializeField] private float spinningSpeed = 300f;
    [SerializeField] private bool useInterpolation = true;
    [SerializeField] private float interpolationSpeed = 300f;

    [Tooltip("Used to invert the direction of rotation")]
    [SerializeField] private bool isInverted = false;
    private Coroutine spinCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    private float spinDirection = 0;

    protected override void OnEnableComponent()
    {
        StopSpinCoroutine();
    }

    protected override void DoComponentLogic()
    {
        spinCoroutine ??= StartCoroutine(DoSpin()); //if not null start the coroutine and assign it to spinCoroutine
    }

    private IEnumerator DoSpin()
    {
        while (true)
        {

            spinDirection = 0f;

            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;
                if (velocity.sqrMagnitude > 0.0001f)
                {
                    spinDirection = Mathf.Sign(velocity.normalized.x);
                }
            }

            float directionMultiplier = isInverted ? 1f : -1f;
            float rotationAmount = spinningSpeed * Time.deltaTime * directionMultiplier;

            if (spinDirection < 0f)
            {
                rotationAmount *= -1f;
            }

            if (useInterpolation)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z + rotationAmount);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(0f, 0f, rotationAmount);
            }

            yield return waitForFixedUpdate;
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
